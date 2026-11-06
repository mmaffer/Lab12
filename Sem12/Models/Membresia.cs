using System.ComponentModel.DataAnnotations;

namespace Sem12.Models;

public class Membresia
{
    [Key]
    public int IdMembresia { get; set; }
    [Required, StringLength(50)]
    public string Plan { get; set; } = string.Empty;
    [Required, StringLength(30)]
    public string TipoAcceso { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string Turno { get; set; } = string.Empty;
    [Required, Range(1, 36)]
    public int DuracionMeses { get; set; }
    [Required, Range(typeof(decimal), "0.01", "99999.99")]
    public decimal Precio { get; set; }
    [Required, Range(1, int.MaxValue)]
    public int IdSocio { get; set; }
    public Socio? Socio { get; set; }
}
