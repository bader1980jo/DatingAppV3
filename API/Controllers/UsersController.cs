using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualBasic;
using Oracle.ManagedDataAccess.Client;

namespace API.Controllers
{

    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        public UsersController(DataContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [Authorize]
        //api/users/3
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            return await _context.Users.FindAsync(id);
        }


        [HttpGet("read")]
        public IActionResult Read()
        {
            IFileProvider fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            var file = fileProvider.GetDirectoryContents("/dtos");
            var latestFile = file.OrderByDescending(f => f.LastModified).FirstOrDefault();

            return Ok(latestFile?.Name);
        }

        [HttpGet("readfile")]
        public async Task<IActionResult> ReadFile()
        {
            string lines = await System.IO.File.ReadAllTextAsync("/projects/DatingApp/API/dtos/logindto.cs");

            return Ok(lines);
        }

        [HttpGet("emirates")]
        public async Task<ActionResult<IEnumerable<Emirates>>> Emirates()
        {
            using (OracleConnection conn = 
                new OracleConnection
                (_config.GetConnectionString("OracleConnection")))
            {
                try
                {
                await conn.OpenAsync();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from emirates";
                DbDataReader reader = await cmd.ExecuteReaderAsync();
                List<Emirates> emirates = new List<Emirates>();

                while (await reader.ReadAsync())
                {
                    Emirates c = new Emirates();
                    c.Code = int.Parse(await reader.IsDBNullAsync(0) ? null : reader.GetString(0));
                    c.Description = await reader.IsDBNullAsync(1) ? null : reader.GetString(1);
                    c.Description_Eng = await reader.IsDBNullAsync(2) ? null : reader.GetString(2);

                    emirates.Add(c);
                }
                await reader.DisposeAsync();
                return emirates;
                }
                catch (Exception e)
                {
                    return BadRequest(" Get error while retrieving emirates data, exception: " + e.Message);
                    
                }

            }
        }
    }
}