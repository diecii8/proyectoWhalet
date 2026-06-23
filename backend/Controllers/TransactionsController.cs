using BackEndWallet.Data;
using BackEndWallet.Data;
using BackEndWallet.Models;
using BackEndWallet.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;

namespace BackEndWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext _contexto;

        public TransactionsController(ApplicationDbContext contexto)
        {
            this._contexto = contexto;
        }

        [HttpGet]
        [Route("obtenerTransacciones/{id}")]
        public JsonResult obtenerTransacciones(int id)
        {
            List<transaccion_dto> lista = new List<transaccion_dto>();
            foreach (var item in _contexto.transacciones.Where(t => t.UsuarioId == id).ToList())
            {
                lista.Add(new transaccion_dto()
                {
                    Id = item.Id,
                    UsuarioId = item.UsuarioId,
                    Operacion = item.Operacion,
                    Moneda = item.Moneda,
                    Monto = item.Monto,
                    Valor = item.Valor,
                    Fecha = item.Fecha
                });
            }
            return Json(lista);
        }

        //public JsonResult obtenerTransaccion(int id)
        //{
        //    var transaccion = _contexto.transacciones
        //        .Where(e => e.Id == id).FirstOrDefault();

        //    var transaccionDTO = new transaccion_dto();
        //    if (transaccion != null)
        //    {

        //        return Json(transaccion);
        //        //transaccionDTO = new transaccion_dto
        //        //{
        //        //    Id = usuario.Id,
        //        //    Nombre = usuario.Nombre,
        //        //    Mail = usuario.Mail,
        //        //    Contraseña = usuario.Contraseña,
        //        //    Tipo = usuario.Tipo
        //        //};
        //    }
        //    return Json(transaccionDTO);
        //}


        [HttpPost]
        [Route("crearTransaccion")]
        public IActionResult crearTransaccion([FromBody] Transacciones transaccion)
        {
            if (!string.IsNullOrWhiteSpace(transaccion.Operacion)
                && !string.IsNullOrWhiteSpace(transaccion.Moneda)
                && transaccion.Monto > 0)
            {
                var billetera = _contexto.billeteras.Where(b => b.UsuarioId == transaccion.UsuarioId).FirstOrDefault();
                if (transaccion.Operacion == "Compra")
                {
                    if (billetera.Saldo >= transaccion.Valor)
                    {
                        _contexto.transacciones.Add(new Transacciones
                        {
                            Operacion = transaccion.Operacion,
                            Moneda = transaccion.Moneda,
                            Monto = transaccion.Monto,
                            Valor = transaccion.Valor,
                            Fecha = transaccion.Fecha,
                            UsuarioId = transaccion.UsuarioId
                        });

                        _ = transaccion.Moneda.ToString().ToUpper() switch
                        {
                            "BITCOIN" => billetera.BitCoin += transaccion.Monto,
                            "ETHEREUM" => billetera.Ethereum += transaccion.Monto,
                            "USDC" => billetera.USDC += transaccion.Monto,
                            "LITECOIN" => billetera.Litecoin += transaccion.Monto,
                            "DOGECOIN" => billetera.Dogecoin += transaccion.Monto,
                            _ => throw new InvalidOperationException("Moneda no válida")
                        };

                        _contexto.SaveChanges();

                        billetera.Saldo -= transaccion.Valor;
                        _contexto.SaveChanges();
                        return Ok(transaccion);
                    }
                    return Ok(new { error = "Saldo insuficiente." });

                }
                if (transaccion.Operacion == "Venta")
                {
                    var moneda = transaccion.Moneda.ToString().ToUpper();
                    var saldoDisponible = ObtenerSaldoMoneda(billetera, moneda);

                    if (saldoDisponible >= transaccion.Monto)
                    {
                        realizarVenta(transaccion);
                        _contexto.SaveChanges();
                            _ = moneda switch
                            {
                                "BITCOIN" => billetera.BitCoin -= transaccion.Monto,
                                "ETHEREUM" => billetera.Ethereum -= transaccion.Monto,
                                "USDC" => billetera.USDC -= transaccion.Monto,
                                "LITECOIN" => billetera.Litecoin -= transaccion.Monto,
                                "DOGECOIN" => billetera.Dogecoin -= transaccion.Monto,
                                _ => throw new InvalidOperationException("Moneda no válida")
                            };
                        _contexto.SaveChanges();
                        billetera.Saldo += transaccion.Valor;
                        _contexto.SaveChanges();
                        return Ok(transaccion);
                    }

                    return Ok(new { error = $"Monto de {moneda} insuficiente." });
                }

            }
                return Ok(new { error = "Datos incompletos." });

        }
                   private decimal ObtenerSaldoMoneda(Billetera billetera, string moneda)
                   {
                        return moneda switch
                        {
                            "BITCOIN" => billetera.BitCoin,
                            "ETHEREUM" => billetera.Ethereum,
                            "USDC" => billetera.USDC,
                            "LITECOIN" => billetera.Litecoin,
                            "DOGECOIN" => billetera.Dogecoin,
                            _ => 0
                        };
                   }
                
                    
            


        public Transacciones realizarVenta(Transacciones transaccion)
        {
            _contexto.transacciones.Add(new Transacciones
            {
                Operacion = transaccion.Operacion,
                Moneda = transaccion.Moneda,
                Monto = transaccion.Monto,
                Valor = transaccion.Valor,
                Fecha = transaccion.Fecha,
                UsuarioId = transaccion.UsuarioId
            });

            return transaccion;
        }

        [HttpPut]
        [Route("editarTransaccion/{id}")]
        public IActionResult editarTransacciones(int id, [FromBody] Transacciones transaccion)
        {
            var tran = _contexto.transacciones.FirstOrDefault(x => x.Id == id);

            if (tran == null)
                return NotFound();

            tran.Operacion = transaccion.Operacion;
            tran.Moneda = transaccion.Moneda;
            tran.Monto = transaccion.Monto;
            tran.Valor = transaccion.Valor;
            tran.Fecha = transaccion.Fecha;
            tran.UsuarioId = transaccion.UsuarioId;
            _contexto.SaveChanges();

            return Ok(tran);
        }

        [HttpDelete]
        [Route("borrarTransaccion/{id}")]
        public IActionResult borrarTransaccion(int id)
        {
            var tran = _contexto.transacciones.FirstOrDefault(x => x.Id == id);

            if (tran == null)
                return Ok(new { error = "Transacción no pudo borrarse." });

            _contexto.transacciones.Remove(tran);
            _contexto.SaveChanges();

            return Ok(new { estado = "ok" });
        }


    }
}