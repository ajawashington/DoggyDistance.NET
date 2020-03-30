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
    public class WalkController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkController(IConfiguration config)
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
            [FromQuery] int? walkDuration,
            [FromQuery] DateTime? date,
            [FromQuery] int? dogId,
            [FromQuery] int? walkerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, WalkDuration, Date, DogId, WalkerId FROM Walks WHERE 1 = 1";

                    if (walkDuration != null)
                    {
                        cmd.CommandText += " AND WalkDuration = @walkDuration";
                        cmd.Parameters.Add(new SqlParameter("@walkDuration", "%" + walkDuration + "%"));
                    }

                    if  (date != null)
                    {
                        cmd.CommandText += " AND Date = @date";
                        cmd.Parameters.Add(new SqlParameter(" date", "%" + date + "%"));
                    }

                    if (dogId != null)
                    {
                        cmd.CommandText += " AND DogId = @dogId";
                        cmd.Parameters.Add(new SqlParameter("@dogId", dogId));
                    }

                    if (walkerId != null)
                    {
                        cmd.CommandText += " AND WalkerId = @walkerId";
                        cmd.Parameters.Add(new SqlParameter("@walkerId", walkerId));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Walk> walks = new List<Walk>();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        int dogIdValue = reader.GetInt32(reader.GetOrdinal("DogId"));
                        int walkDurationValue = reader.GetInt32(reader.GetOrdinal("WalkDuration"));
                        DateTime dateValue = reader.GetDateTime(reader.GetOrdinal("Date"));
                        int walkerIdValue = reader.GetInt32(reader.GetOrdinal("WalkerId"));

                        Walk walk = new Walk
                        {
                            Id = id,
                            WalkDuration = walkDurationValue,
                            Date = dateValue,
                            WalkerId = walkerIdValue,
                            DogId = dogIdValue

                        };

                        walks.Add(walk);
                    }
                    reader.Close();

                    return Ok(walks);

                }
            }
        }


        [HttpGet("{id}", Name = "GetWalk")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT Id, WalkDuration, Date, DogId, WalkerId FROM Walks
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walk walk = null;

                    if (reader.Read())
                    {
                        walk = new Walk
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            WalkDuration = reader.GetInt32(reader.GetOrdinal("WalkDuration")),
                            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                            WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId"))
                        };
                    }
                    reader.Close();

                    return Ok(walk);
                }
            }
        }

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Walk walk)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Walks (WalkDuration, Date, DogId, WalkerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@WalkDuration, @Date, @DogId, @WalkerId)";
                    cmd.Parameters.Add(new SqlParameter("@WalkDuration", walk.WalkDuration));
                    cmd.Parameters.Add(new SqlParameter("@Date", walk.Date));
                    cmd.Parameters.Add(new SqlParameter("@DogId", walk.DogId));
                    cmd.Parameters.Add(new SqlParameter("@WalkerId", walk.WalkerId));

                    int newId = (int)cmd.ExecuteScalar();
                    walk.Id = newId;
                    return CreatedAtRoute("GetWalk", new { id = newId }, walk);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walk walk)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Walks
                                            SET WalkDuration = @WalkDuration,
                                                Date = @Date,
                                                WalkerId = @WalkerId
                                                DogId = @DogId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@WalkDuration", walk.WalkDuration));
                        cmd.Parameters.Add(new SqlParameter("@Date", walk.Date));
                        cmd.Parameters.Add(new SqlParameter("@WalkerId", walk.WalkerId));
                        cmd.Parameters.Add(new SqlParameter("@DogId", walk.DogId));
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
                if (!WalkExists(id))
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
                        cmd.CommandText = @"DELETE FROM Walks WHERE Id = @id";
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
                if (!WalkExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool WalkExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, WalkDuration, Date, DogId, WalkerId
                        FROM Walks
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
