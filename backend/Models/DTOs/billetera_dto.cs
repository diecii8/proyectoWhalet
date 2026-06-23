using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndWallet.Models.DTOs
{
    public class billetera_dto
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
        [Column(TypeName = "decimal(18,8)")]
        public decimal Saldo { get; set; } = 0;
        public int UsuarioId { get; set; }
    }
}
