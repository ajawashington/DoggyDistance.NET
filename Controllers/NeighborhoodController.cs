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
    public class NeighborhoodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NeighborhoodController(IConfiguration config)
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
           [FromQuery] string neighborhoodName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" SELECT Id, NeighborhoodName FROM Neighborhood WHERE 1 = 1";

                    if (neighborhoodName != null)
                    {
                        cmd.CommandText += " AND NeighborhoodName LIKE @neighborhoodName";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodName", "%" + neighborhoodName + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Neighborhood> hoods = new List<Neighborhood>();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string neighborhoodNameValue = reader.GetString(reader.GetOrdinal("NeighborhoodName"));

                        Neighborhood hood = new Neighborhood
                        {

                            Id = id,
                            NeighborhoodName = neighborhoodNameValue,
                        };

                        hoods.Add(hood);
                    }
                    reader.Close();

                    return Ok(hoods);

                }
            }
        }

        [HttpGet("{id}", Name = "GetNeighborhood")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT Id, NeighborhoodName, NeighborhoodId FROM Neighborhood
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Neighborhood hood = null;

                    if (reader.Read())
                    {
                        hood = new Neighborhood
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            NeighborhoodName = reader.GetString(reader.GetOrdinal("NeighborhoodName")),
                        };
                    }
                    reader.Close();

                    return Ok(hood);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Neighborhood hood)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Neighborhood (NeighborhoodName)
                                        OUTPUT INSERTED.Id
                                        VALUES (@NeighborhoodName, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodName", hood.NeighborhoodName));

                    int newId = (int)cmd.ExecuteScalar();
                    hood.Id = newId;
                    return CreatedAtRoute("GetNeighborhood", new { id = newId }, hood);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Neighborhood hood)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = @"UPDATE Neighborhood
                                            SET NeighborhoodName = @NeighborhoodName
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@NeighborhoodName", hood.NeighborhoodName));
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
                if (!NeighborhoodExists(id))
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
                        cmd.CommandText = @"DELETE FROM Neighborhood WHERE Id = @id";
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
                if (!NeighborhoodExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool NeighborhoodExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, NeighborhoodName 
                        FROM Neighborhood
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

