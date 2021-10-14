﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NETCore.Repository.StaticMethod;
using Overtime.Base;
using Overtime.Context;
using Overtime.Models;
using Overtime.Repository.Data;
using Overtime.ViewModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Overtime.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : BaseController<Account, AccountRepository, string>
    {
        private readonly AccountRepository repository;
        private readonly UserRepository userRepository;
        public IConfiguration _configuration;
        private readonly MyContext myContext;
        Role role = new Role();

        public AccountsController(AccountRepository repository, IConfiguration configuration, MyContext myContext) : base(repository)
        {
            this.repository = repository;
            _configuration = configuration;
            this.myContext = myContext;
        }
        [HttpGet("GetLogin")]
        public ActionResult GetLogin()
        {
            var getLogin = repository.GetLoginVMs();
            if (getLogin == null)
            {
                return NotFound(new
                {
                    status = HttpStatusCode.NotFound,
                    result = getLogin,
                    message = "Data Kosong"
                });
            }
            else
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    result = getLogin,
                    message = "Success"
                });
            }

        }
        [HttpGet("GetManager")]
        public ActionResult GetManagerName()
        {
            var get = repository.GetManagerName();
            if (get != null)
            {
                return Ok(repository.GetManagerName());
            }

            return NotFound("Tidak ada Data");

        }
        [HttpPost("Login")]
        public ActionResult Login(LoginVM login)
        {
            try
            {
                //check data by email
                var checkdata = repository.Login(login);
                if (checkdata == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new
                    {
                        status = (int)HttpStatusCode.BadRequest,
                        message = "Email tidak ditemukan"
                    });
                }

                //check password bycrpt
                if (!BCrypt.Net.BCrypt.Verify(login.Password, checkdata.Password))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new
                    {
                        status = (int)HttpStatusCode.BadRequest,
                        message = "Password Salah"
                    });
                }
                else
                {   
                    string[] roles = repository.Roles(login.Email);
                    var claim = new List<Claim>();
                    claim.Add(new Claim(ClaimTypes.Email, login.Email));
                    claim.Add(new Claim(ClaimTypes.Name, repository.GetName(login.Email)));
                    claim.Add(new Claim(ClaimTypes.NameIdentifier, repository.GetId(login.Email)));
                    foreach (string d in roles)
                    {
                        claim.Add(new Claim("roles", d));
                    }
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claim, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                    return Ok(new JWTokenVM 
                      { Token = new JwtSecurityTokenHandler().WriteToken(token), 
                        Id = repository.GetId(login.Email),
                        Salary = repository.GetSalary(login.Email),
                        Messages = "Login Berhasil"
                    });
                }

            }
            catch (System.Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    status = (int)HttpStatusCode.InternalServerError,
                    message = e.Message
                });
            }
        }
        [HttpPost("SendPasswordReset")]
        public ActionResult SendPasswordReset(LoginVM loginVM)
        {
            //validating
            if (string.IsNullOrEmpty(loginVM.Email))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    status = (int)HttpStatusCode.BadRequest,
                    message = "Email tidak boleh null atau kosong"
                });
            }

            try
            {
                //check email
                var account = repository.FindByEmail(loginVM.Email);

                if (account == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new
                    {
                        status = (int)HttpStatusCode.BadRequest,
                        message = "Email tidak terdaftar"
                    });
                }
                //Generate Reset password with GUID
                string resetPassword = Guid.NewGuid().ToString();
                string GetDate = DateTime.Now.ToString();
                string SubjectMail = $"Reset Password - {GetDate}";

                //Reset password
                if (repository.ResetPassword(account.Id, resetPassword))
                {
                    //send password to email
                    EmailSender.SendEmail(loginVM.Email, SubjectMail, "Hello "
                                  + loginVM.Email + "<br><br>berikut reset password anda, jangan lupa ganti dengan password baru<br><br><b>"
                                  + resetPassword + "<b><br><br>Thanks<br>netcore-api.com");

                    return StatusCode((int)HttpStatusCode.OK, new
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "reset Password berhasil dikirim ke email " + loginVM.Email + "."
                    });
                }

                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    status = (int)HttpStatusCode.InternalServerError,
                    message = "Gagal reset password"
                });

            }
            catch (System.Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    status = (int)HttpStatusCode.InternalServerError,
                    message = e.Message
                });
            }
        }
        [HttpPost("ChangePassword")]
        public ActionResult ChangePassword(LoginVM loginVM)
        {
            //validating
            if (string.IsNullOrEmpty(loginVM.Email) || string.IsNullOrEmpty(loginVM.Password) || string.IsNullOrEmpty(loginVM.NewPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    status = (int)HttpStatusCode.BadRequest,
                    message = "Email dan password tidak boleh null atau kosong"
                });
            }

            //check email
            var account = repository.FindByEmail(loginVM.Email);

            if (account == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    status = (int)HttpStatusCode.BadRequest,
                    message = "Email tidak terdaftar"
                });
            }

            //check password match
            if (!BCrypt.Net.BCrypt.Verify(loginVM.Password, account.Password))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    status = (int)HttpStatusCode.BadRequest,
                    message = "Password Salah"
                });
            }

            //change password
            repository.Update(new Account
            {
                Id = account.Id,
                Password = BCrypt.Net.BCrypt.HashPassword(loginVM.NewPassword)
            });

            return StatusCode((int)HttpStatusCode.OK, new
            {
                status = (int)HttpStatusCode.OK,
                message = "ubah password berhasil"
            });
        }
    }
}
