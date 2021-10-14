﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Overtime.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Overtime.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<Entity, Repository, Key> : ControllerBase
        where Entity : class
        where Repository : IRepository<Entity, Key>
    {
        private readonly Repository repository;
        public BaseController(Repository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public ActionResult Insert(Entity entity)
        {
            try
            {
                if (repository.Insert(entity) > 0)
                {
                    return Ok(new { status = HttpStatusCode.OK, message = "Data Berhasil ditambahkan" });
                }
                else if (repository.Insert(entity) == 0)
                {
                    return BadRequest(new { status = HttpStatusCode.BadRequest, message = "Gagal Menambahkan Data" });
                }
                else
                {
                    return BadRequest(new { status = HttpStatusCode.BadRequest, message = "Data Sudah ada" });
                }
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    status = (int)HttpStatusCode.InternalServerError,
                    message = e.InnerException.Message
                });
            }
        }

        [HttpGet]
        public ActionResult<Entity> Get()
        {
            if (repository.Get() != null)
            {

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = repository.Get(),
                    message = "Data berhasil Di tampilkan"
                });
            }
            else
            {
                return NotFound("Data Tidak Ada");
            }
        }

        [HttpGet("{key}")]
        public ActionResult Get(Key key)
        {
            if (repository.Get(key) != null)
            {

                //return Ok(personRepository.Get(NIK));
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = repository.Get(key),
                    message = "Data berhasil Di tampilkan"
                });
            }
            return NotFound(new
            {
                status = HttpStatusCode.NotFound,
                message = "Data Tidak Ditemukan"
                //return Ok(personRepository.Get(NIK));
            });
        }

        [HttpPut]
        public ActionResult Update(Entity entity)
        {
            try
            {
                if (repository.Update(entity) != 0)
                {
                    //return Ok("Data Berhasil di Update");
                    return Ok(new
                    {
                        status = HttpStatusCode.OK,
                        //data = repository.Get(entity.key),
                        message = "Data berhasil Di Update"
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return NotFound(new
            {
                status = HttpStatusCode.NotFound,
                message = "Data dengan NIK tersebut Tidak Ditemukan"

            });
        }

        [HttpDelete("{key}")]
        public ActionResult Delete(Key key)
        {
            if (repository.Get(key) != null)
            {
                //personRepository.Delete(NIK);
                //return Ok("Data Berhasil Dihapus");  
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = repository.Get(key),
                    deletedata = repository.Delete(key),
                    message = "Data berhasil Di Hapus"
                });
            }
            else
            {
                return BadRequest(new
                {
                    status = HttpStatusCode.NotFound,
                    message = "Data dengan NIK tersebut Tidak Ditemukan"
                });
            }
        }
    }
}
