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
    public class DogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogController(IConfiguration config)
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
                    cmd.CommandText = "SELECT d.Id, d.DogName, d.Breed, d.DogOwnerId, d.Notes, do.OwnerName FROM Dog d INNER JOIN DogOwner do ON d.DogOwnerId = do.Id  ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            DogOwnerId = reader.GetInt32(reader.GetOrdinal("DogOwnerId")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            OwnerName = reader.GetString(reader.GetOrdinal("OwnerName"))
                        };

                        dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogs);

                }
            }
        }


        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT Id, DogName, Breed, DogOwnerId, Notes FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            DogOwnerId = reader.GetInt32(reader.GetOrdinal("DogOwnerId"))
                        };
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog (DogName, Breed, DogOwnerId, Notes)
                                        OUTPUT INSERTED.Id
                                        VALUES (@DogName, @Breed, @DogOwnerId, @Notes)";
                    cmd.Parameters.Add(new SqlParameter("@DogName", dog.DogName));
                    cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@DogOwnerId", dog.DogOwnerId));
                    cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog
                                            SET DogName = @DogName,
                                                Breed = @Breed,
                                                Notes = @Notes
                                                DogOwnerId = @DogOwnerId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@DogName", dog.DogName));
                        cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));
                        cmd.Parameters.Add(new SqlParameter("@DogOwnerId", dog.DogOwnerId));
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
                if (!DogExists(id))
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
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
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
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, DogName, Breed, DogOwnerId, Notes
                        FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
