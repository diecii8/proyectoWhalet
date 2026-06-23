using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndWallet.Models
{
    public class Transacciones
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuarios? Usuario { get; set; }
        public string Operacion { get; set; }
        public string Moneda { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Monto { get; set; }
        public decimal Valor { get; set; }
        public DateTime Fecha { get; set; }
    }
}
