using System.Windows.Forms;

namespace DetectObject.Test
{
    public static class ThreadHelper
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text);
        delegate void AddItemCallback(Form f, ListBox ctrl, string text);

        public static void SetText(Form form, Control ctrl, string text)
        {
            if (ctrl.InvokeRequired)
            {
                var d = new SetTextCallback(SetText);
                form.Invoke(d, form, ctrl, text);
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void AddLogItem(Form form, ListBox ctrl, string text)
        {
            if (form.IsDisposed || ctrl.IsDisposed)
            {
                return;
            }

            if (ctrl.InvokeRequired)
            {
                var d = new AddItemCallback(AddLogItem);
                form.Invoke(d, form, ctrl, text);

            }
            else
            {
                ctrl.Items.Add(text);
                if (ctrl.Items.Count > 6)
                {
                    ctrl.Items.RemoveAt(0);
                }
            }
        }
    }
}
