using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndWallet.Models
{
    public class Billetera
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal BitCoin { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Ethereum { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal USDC { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Litecoin { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Dogecoin { get; set; }
        public decimal Saldo { get; set; } = 0;
        public int UsuarioId { get; set; }
        public Usuarios? Usuario { get; set; }
    }
}
