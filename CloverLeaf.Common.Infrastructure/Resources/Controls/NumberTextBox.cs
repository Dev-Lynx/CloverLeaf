using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CloverLeaf.Common.Infrastructure.Resources.Controls
{
    public class NumberTextBox : TextBox
    {
        public double MinValue { get; set; } = int.MinValue;
        public double MaxValue { get; set; } = int.MaxValue;
        public bool NaNEnabled { get; set; } = false;

        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!double.TryParse(Text + e.Text, out double d))
                e.Handled = true;
            if (d < MinValue || d > MaxValue) e.Handled = true;

            base.OnPreviewTextInput(e);
        }
    }
}
