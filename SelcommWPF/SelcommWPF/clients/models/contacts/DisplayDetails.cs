using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class DisplayDetails
    {

        public string Key { get; set; }

        public string Label { get; set; }

        public string Value { get; set; }

        public long? MenuId { get; set; }

        public string DataType { get; set; }

        public string IconURL { get; set; }

        public string Tooltip { get; set; }

        public int DisplayOrder { get; set; }

        public bool DisplayBold { get; set; }

        public string DisplayColor { get; set; }

        public string DisplayGroup { get; set; }

        public string NavigationPath { get; set; }

        public string EditType { get; set; }

        public string ApiURL { get; set; }

        public string AttributeType { get; set; }

        public string AttributeKey { get; set; }

        public string AttributeValue { get; set; }

        public List<MenuModel.MenuItem> MenuItems { get; set; }

        public string ObjectId { get; set; }


    }
}
