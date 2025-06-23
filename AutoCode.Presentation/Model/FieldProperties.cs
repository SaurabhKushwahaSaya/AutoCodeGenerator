using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCode.Presentation.Model
{
    class FieldProperties
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string FieldType { get; set; } = "TextBox";
        public bool IsSortable { get; set; } = true;
        public bool IsRequired { get; set; } = true;
        public bool IsShow { get; set; } = true;
        public bool IsDisable { get; set; } = false;
       // public string ValidationMessage { get; set; } = string.Empty;
        public bool IsEditAllow { get; set; } = false;
        public bool IsDeleteAllow { get; set; } = false;
    }
}
