using BackEndWallet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using BackEndWallet.Data;
using BackEndWallet.Models;
using BackEndWallet.Models.DTOs;

namespace BackEndWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : Controller
    {
        private ApplicationDbContext _contexto;

        public UsuariosController(ApplicationDbContext contexto)
        {
            this._contexto = contexto;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("API escuchando...");
        }
        //public JsonResult obtenerUsuarios()
        //{
        //    List<usuario_dto> lista = new List<usuario_dto>();
        //    foreach (var item in _contexto.usuarios.ToList())
        //    {
        //        lista.Add(new usuario_dto() {
        //            Nombre = item.Nombre,
        //            Mail = item.Mail,
        //            Tipo = item.Tipo
        //        });
        //    }
        //    return Json(lista);
        //}

        public JsonResult obtenerUsuario(int id)
        {
            var usuario = _contexto.usuarios
                .Where(e => e.Id == id).FirstOrDefault();

            var usuarioDTO = new usuario_dto();
            if (usuario != null)
            {

                return Json(usuario);
                //usuarioDTO = new usuario_dto
                //{
                //    Id = usuario.Id,
                //    Nombre = usuario.Nombre,
                //    Mail = usuario.Mail,
                //    Contraseña = usuario.Contraseña,
                //    Tipo = usuario.Tipo
                //};
            }
            return Json(usuarioDTO);
        }

        [HttpPost]
        [Route("validarUsuario")]
        public JsonResult validarUsuario([FromBody] usuario_dto datos)
        {
            var usuario = _contexto.usuarios
                .FirstOrDefault(e =>
                    e.Nombre == datos.Nombre &&
                    e.Contraseña == datos.Contraseña);

            if (usuario != null)
            {
                return Json(new { estado = "ok", usuario });
            }

            return Json(new
            {
                estado = "error", mensaje = "Usuario o contraseña incorrectos."
            });
        }

        [HttpPost]
        [Route("crearUsuario")]
        public JsonResult crearUsuario([FromBody] Usuarios usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Nombre)
                && !string.IsNullOrWhiteSpace(usuario.Mail)
                && !string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                if (_contexto.usuarios
                    .Where(e => e.Mail == usuario.Mail)
                    .FirstOrDefault() == null)
                {
                    _contexto.usuarios.Add(usuario);
                    _contexto.SaveChanges();

                    return Json(new { estado = "ok", usuario });
                }
                return Json(new { error = "Usuario existente."});
            }
            return Json(new { error = "Datos incompletos." });
        }

        [HttpPut]
        public IActionResult editarUsuario(int id, [FromBody] Usuarios usuario)
        {
            var usu = _contexto.usuarios.FirstOrDefault(x => x.Id == id);

            if (usu == null)
                return NotFound();

            usu.Nombre = usuario.Nombre;            
            usu.Contraseña = usuario.Contraseña;
            _contexto.SaveChanges();

            return Ok(usu);
        }

        [HttpDelete]
        public IActionResult borrarUsuario(int id)
        {
            var usu = _contexto.usuarios.FirstOrDefault(x => x.Id == id);

            if (usu == null)
                return Ok(new { error = "Usuario no pudo borrarse." });

            _contexto.usuarios.Remove(usu);
            _contexto.SaveChanges();

            return Ok(new { estado = "ok" });
        }


    }
}
