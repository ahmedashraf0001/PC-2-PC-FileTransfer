public class CustomPanel : Panel
{
    //public int CornerRadius { get; set; } = 15; // Default corner radius
    //public Color BorderColor { get; set; } = Color.FromArgb(220, 220, 220); // Default border color

    //protected override void OnPaint(PaintEventArgs e)
    //{
    //    base.OnPaint(e);

    //    // Set up a graphics object to draw the rounded rectangle
    //    var rect = new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
    //    var path = new System.Drawing.Drawing2D.GraphicsPath();
    //    //path.AddArc(rect.X, rect.Y, CornerRadius, CornerRadius, 180, 90);
    //    //path.AddArc(rect.Width - CornerRadius, rect.Y, CornerRadius, CornerRadius, 270, 90);
    //    //path.AddArc(rect.Width - CornerRadius, rect.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
    //    //path.AddArc(rect.X, rect.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
    //    path.CloseAllFigures();

    //    // Draw the background and border
    //    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    //    e.Graphics.FillPath(new SolidBrush(this.BackColor), path);
    //    e.Graphics.DrawPath(new Pen(BorderColor), path);
    //}
}