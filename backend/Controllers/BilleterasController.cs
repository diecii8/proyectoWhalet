using BackEndWallet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using BackEndWallet.Data;
using BackEndWallet.Models;
using BackEndWallet.Models.DTOs;

namespace BackEndWallet.Controllers
{

    public class BilleterasController : Controller
    {
        private ApplicationDbContext _contexto;

        public BilleterasController(ApplicationDbContext contexto)
        {
            this._contexto = contexto;
        }

        public string Index()
        {
            return "API escuchando...";
        }
        public JsonResult obtenerBilleteras()
        {
            List<billetera_dto> lista = new List<billetera_dto>();
            foreach (var item in _contexto.billeteras.Include(e => e.Usuario).ToList())
            {
                lista.Add(new billetera_dto
                {
                    Id = item.Id,
                    UsuarioId = item.UsuarioId,
                    Saldo = item.Saldo,
                    BitCoin = item.BitCoin,
                    Ethereum = item.Ethereum,
                    USDC = item.USDC,
                    Litecoin = item.Litecoin,
                    Dogecoin = item.Dogecoin
                });

            }
            return Json(lista);
        }

        [HttpGet]
        [Route("api/billeteras/obtener/{idUsuario}")]
        public JsonResult obtenerBilletera(int idUsuario)
        {
            var billetera = _contexto.billeteras
                .Where(e => e.UsuarioId == idUsuario).FirstOrDefault();
            var billeteraDTO = new billetera_dto();
            if (billetera != null)
            {

                return Json(billetera);
                //billeteraDTO = new billetera_dto
                //{
                //    Id = billetera.Id,
                //    UsuarioId = billetera.UsuarioId,
                //    Saldo = billetera.Saldo,
                //    BitCoin = billetera.BitCoin,
                //    Ethereum = billetera.Ethereum,
                //    USDC = billetera.USDC,
                //    Litecoin = billetera.Litecoin,
                //    Dogecoin = billetera.Dogecoin
                //};
                
            }
            return Json(billeteraDTO);
        }

        [HttpPost]
        [Route("api/billeteras/crear/{UsuarioId}")]
        public IActionResult crearBilletera(int UsuarioId)
        {
            if (UsuarioId > 0)
            {
                if (_contexto.billeteras
                    .Where(e => e.UsuarioId == UsuarioId)
                    .FirstOrDefault() == null)
                {
                    var billetera = new Billetera
                    {
                        UsuarioId = UsuarioId,
                        Saldo = 500000,
                        BitCoin = 0,
                        Ethereum = 0,
                        USDC = 0,
                        Litecoin = 0,
                        Dogecoin = 0
                    };
                    _contexto.billeteras.Add(billetera);
                    _contexto.SaveChanges();

                    return Ok(billetera);
                }
               return Ok(new { error = "Billetera existente." });
            }

            return Ok(new { error = "Usuario no existe." });
        }

        [HttpPut]
        [Route("api/billeteras/editar/{id}")]
        public IActionResult editarBilletera(int id, [FromBody] Billetera billetera)
        {
            var bille = _contexto.billeteras.FirstOrDefault(x => x.Id == id);

            if (bille == null)
                return NotFound();

            bille.UsuarioId = billetera.UsuarioId;
            bille.Saldo = billetera.Saldo;
            bille.BitCoin = billetera.BitCoin;
            bille.Ethereum = billetera.Ethereum;
            bille.USDC = billetera.USDC;
            bille.Litecoin = billetera.Litecoin;
            bille.Dogecoin = billetera.Dogecoin;
            _contexto.SaveChanges();

            return Ok(bille);
        }

        [HttpDelete]
        [Route("api/billeteras/borrar/{id}")]
        public IActionResult borrarBilletera(int id)
        {
            var bille = _contexto.billeteras.FirstOrDefault(x => x.Id == id);

            if (bille == null)
                return Ok(new { error = "Billetera no pudo borrarse." });

            _contexto.billeteras.Remove(bille);
            _contexto.SaveChanges();

            return Ok(new { estado = "ok" });
        }


    }
}
