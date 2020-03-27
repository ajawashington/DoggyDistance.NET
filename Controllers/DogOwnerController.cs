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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = " SELECT Id, OwnerName, OwnerAddress, NeighborhoodId, PhoneNumber FROM DogOwner ";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<DogOwner> dogOwners = new List<DogOwner>();

                    while (reader.Read())
                    {
                        DogOwner dogOwner = new DogOwner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                            OwnerAddress = reader.GetString(reader.GetOrdinal("OwnerAddress")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        
                        };

                        dogOwners.Add(dogOwner);
                    }
                    reader.Close();

                    return Ok(dogOwners);

                }
            }
        }


        [HttpGet("{id}", Name = "GetDogOwner")]
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

                    DogOwner dogOwner = null;

                    while (reader.Read())
                    {
                        if (dogOwner == null)
                        {
                            dogOwner = new DogOwner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                                OwnerAddress = reader.GetString(reader.GetOrdinal("OwnerAddress")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                dogs = new List<Dog>()

                            };
                        }
                        var dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            //Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            DogOwnerId = reader.GetInt32(reader.GetOrdinal("Id")),
                            //Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            //OwnerName = reader.GetString(reader.GetOrdinal("OwnerName"))
                        };      

                        //To do: Add "if" statement to find out "breed or notes" is not null and if it isn't then set that property on dog 
                        dogOwner.dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogOwner);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DogOwner dogOwner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO DogOwner (OwnerName, OwnerAddress, PhoneNumber, NeighborhoodId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@OwnerName, @Address, @PhoneNumber, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@OwnerName", dogOwner.OwnerName));
                    cmd.Parameters.Add(new SqlParameter("@OwnerAddrees", dogOwner.OwnerAddress));
                    cmd.Parameters.Add(new SqlParameter("@PhoneNumber", dogOwner.PhoneNumber));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", dogOwner.NeighborhoodId));

                    int newId = (int)cmd.ExecuteScalar();
                    dogOwner.Id = newId;
                    return CreatedAtRoute("GetDogOwner", new { id = newId }, dogOwner);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] DogOwner dogOwner)
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
                        cmd.Parameters.Add(new SqlParameter("@OwnerName", dogOwner.OwnerName));
                        cmd.Parameters.Add(new SqlParameter("@OwnerAddrees", dogOwner.OwnerAddress));
                        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", dogOwner.PhoneNumber));
                        cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", dogOwner.NeighborhoodId));
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
                if (!DogOwnerExists(id))
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
                        cmd.CommandText = @"DELETE FROM DogOwner WHERE Id = @id";
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
                if (!DogOwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogOwnerExists(int id)
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

