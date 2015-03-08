using System.Drawing;
using System.Windows.Forms;

using Sulakore.Extensions;

namespace Sulakore.Components
{
    public class SKoreForm : Form
    {
        /// <summary>
        /// Gets or sets the extension for this instance.
        /// </summary>
        public ExtensionBase Extension { get; set; }

        public SKoreForm()
        { }
        public SKoreForm(ExtensionBase extension)
            : this()
        {
            Extension = extension;
        }
    }
}