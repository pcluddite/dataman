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
            SuspendLayout();
            if (FormBorderStyle == FormBorderStyle.None)
            {
                Rectangle oldBounds = DesktopBounds;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                Point diff = new Point(DesktopBounds.Width - oldBounds.Width, DesktopBounds.Height - oldBounds.Height);
                SetDesktopLocation(DesktopLocation.X - diff.X, DesktopLocation.Y - diff.Y);
            }
            else
            {
                Rectangle oldBounds = DesktopBounds;
                FormBorderStyle = FormBorderStyle.None;
                Point diff = new Point(oldBounds.Width - DesktopBounds.Width, oldBounds.Height - DesktopBounds.Height);
                SetDesktopLocation(DesktopLocation.X + diff.X, DesktopLocation.Y + diff.Y);
            }
            ResumeLayout();
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
