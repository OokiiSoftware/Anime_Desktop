using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anime.Modelo
{
    public class Error
    {
        public Error(string ItemId = "", string Descricao = "") {
            this.ItemId = ItemId;
            this.Descricao = Descricao;
        }
        public string ItemId { get; set; }
        public string Descricao { get; set; }
    }
}
