using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using DoggyDistance.Models;
using Microsoft.AspNetCore.Http;


namespace DoggyDistance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogOwnerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogOwnerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string ownerName,
            [FromQuery] string ownerAddress, 
            [FromQuery] string phoneNumber, 
            [FromQuery] int? neighborhoodId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" SELECT Id, OwnerName, OwnerAddress, NeighborhoodId, PhoneNumber FROM DogOwner WHERE 1 = 1";

                    if ( ownerName != null)
                    {
                        cmd.CommandText += " AND OwnerName LIKE @ownerName";
                        cmd.Parameters.Add(new SqlParameter("@ownerName", "%" + ownerName + "%"));
                    }

                    if (ownerAddress != null)
                    {
                        cmd.CommandText += " AND OwnerAddress LIKE @ownerAddress";
                        cmd.Parameters.Add(new SqlParameter("@ownerAddress", "%" + ownerAddress + "%"));
                    }

                    if (phoneNumber != null)
                    {
                        cmd.CommandText += " AND PhoneNumber LIKE @phoneNumber";
                        cmd.Parameters.Add(new SqlParameter("@phoneNumber", "%" + phoneNumber + "%"));
                    }

                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " AND NeighborhoodId = @neighborhoodId";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", neighborhoodId));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    List<DogOwner> owners = new List<DogOwner>();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string ownerNameValue = reader.GetString(reader.GetOrdinal("OwnerName"));
                        string ownerAddressValue = reader.GetString(reader.GetOrdinal("OwnerAddress"));
                        int hoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"));
                        string phoneNumberValue = reader.GetString(reader.GetOrdinal("PhoneNumber"));

                        DogOwner owner = new DogOwner
                        {

                            Id = id, 
                            OwnerName = ownerNameValue, 
                            OwnerAddress = ownerAddressValue, 
                            NeighborhoodId = hoodId, 
                            PhoneNumber = phoneNumberValue


                        };

                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);

                }
            }
        }


        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT do.Id, do.OwnerName, do.OwnerAddress, do.NeighborhoodId, do.PhoneNumber, d.DogName, d.Breed, d.Id AS DogId FROM DogOwner do
                    INNER JOIN Dog d ON d.DogOwnerId = do.id WHERE DogOwnerId = 1 ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    DogOwner owner = null;

                    if (reader.Read())
                    {
                        owner = new DogOwner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                            OwnerAddress = reader.GetString(reader.GetOrdinal("OwnerAddress")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DogOwner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO DogOwner (OwnerName, OwnerAddress, PhoneNumber, NeighborhoodId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@OwnerName, @Address, @PhoneNumber, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@OwnerName", owner.OwnerName));
                    cmd.Parameters.Add(new SqlParameter("@OwnerAddrees", owner.OwnerAddress));
                    cmd.Parameters.Add(new SqlParameter("@PhoneNumber", owner.PhoneNumber));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));

                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] DogOwner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = @"UPDATE DogOwner
                                            SET OwnerName = @OwnerName,
                                                OwnerAddress = @OwnerAddress,
                                                PhoneNumber = @PhoneNumber
                                                NeighborhoodId = @NeighborhoodId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@OwnerName", owner.OwnerName));
                        cmd.Parameters.Add(new SqlParameter("@OwnerAddrees", owner.OwnerAddress));
                        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", owner.PhoneNumber));
                        cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));



                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Owner WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, OwnerName, OwnerAddress, PhoneNumber, NeighborhoodId
                        FROM DogOwner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

