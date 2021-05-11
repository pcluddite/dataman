using System.Drawing;
using System.Windows.Forms;

namespace VirtualFlashCards.Forms
{
    public abstract partial class CardFormBase : Form
    {
        protected bool IsDragging { get; set; }
        protected Point DragStartPoint { get; set; }

        public CardFormBase()
        {
            InitializeComponent();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.FixedSingle;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsDragging)
            {
                Point endPoint = PointToScreen(e.Location);
                Location = new Point(endPoint.X - DragStartPoint.X,
                                     endPoint.Y - DragStartPoint.Y);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            IsDragging = true;
            DragStartPoint = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            IsDragging = false;
        }
    }
}
