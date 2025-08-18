using System.Windows.Forms;

namespace NeuracleExtension;

/// <summary>
/// 自定义的提示窗
/// </summary>
public class NeuracleMessageBox : Form
{
    public NeuracleMessageBox(string message, int durationMilliseconds = 1000)
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Size = new System.Drawing.Size(300, 150);
        // 默认置顶
        this.TopMost = true;
        Label label = new()
        {
            Text = message,
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        this.Controls.Add(label);
        // 定时器，durationMilliseconds 后关闭窗体
        Timer timer = new()
        {
            Interval = durationMilliseconds
        };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            this.Close();
        };
        timer.Start();
    }
}
