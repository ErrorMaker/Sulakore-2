using System.Drawing;
using System.Windows.Forms;

using Sulakore.Extensions;

namespace Sulakore.Components
{
    public class SKoreExtensionForm : Form
    {
        /// <summary>
        /// Gets or sets the extension for this instance.
        /// DO NOT use this property in the constructor of your Form unless the base constructor with the ExtensionBase argument is called.
        /// </summary>
        public ExtensionBase Extension { get; set; }

        public SKoreExtensionForm()
        {
            ShowIcon = false;
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
        }
        public SKoreExtensionForm(ExtensionBase extension)
            : this()
        {
            Extension = extension;
        }
    }
}