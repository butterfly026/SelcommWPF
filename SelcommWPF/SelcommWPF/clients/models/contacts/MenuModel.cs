using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class MenuModel
    {

        public long Id { get; set; }

        public string Name { get; set; }

        public List<MenuItem> MenuItems { get; set; }

        public class MenuItem
        {
            public long Id { get; set; }

            public string Caption { get; set; }

            public string ImageUrl { get; set; }

            public string Tooltip { get; set; }

            public int Order { get; set; }

            public int MenuItemGroup { get; set; }

            public bool Checked { get; set; }

            public bool Default { get; set; }

            public string Color { get; set; }

            public string NavigationURL { get; set; }

            public List<MenuItem> MenuItems { get; set; }

        }

    }
}
