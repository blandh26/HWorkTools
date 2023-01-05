using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Windows.Forms.Design;

namespace H_ScreenCapture
{
    public class CaptureToolbarDesigner : ControlDesigner
    {
        public override SelectionRules SelectionRules {
            get {
                return base.SelectionRules & SelectionRules.Moveable;
            }
        }
    }
}
