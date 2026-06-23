namespace BackEndWallet.Models.DTOs
{
    public class transaccion_dto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Operacion { get; set; }
        public string Moneda { get; set; }
        public decimal Monto { get; set; }
        public decimal Valor { get; set; }
        public DateTime Fecha { get; set; }
    }
}
