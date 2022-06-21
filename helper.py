from operator import truediv
import gi
gi.require_version("Gtk", "3.0")
from gi.repository import Gtk, Gdk

class ClipboardWindow(Gtk.Window):
    def __init__(self):
        super().__init__(title="RimWorld", resizable=False)

        self.clipboard = Gtk.Clipboard.get(Gdk.SELECTION_CLIPBOARD)
        self.textentry = Gtk.Entry()
        self.textentry.connect("changed", self.check_text)
        self.textlabel = Gtk.Label(label="Input your text", halign=Gtk.Align.START)
        hintlabel2 = Gtk.Label(label="[Enter or Clicking \"Copy\"] Store text to clipboard.", halign=Gtk.Align.START)
        hintlabel1 = Gtk.Label(label="[Ctrl-J] Exit", halign=Gtk.Align.START)

        box = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=2)
        self.add(box)

        box.pack_start(hintlabel1, True, True, 0)
        box.pack_start(hintlabel2, True, True, 0)
        box.pack_start(self.textentry, True, True, 0)
        box.pack_start(self.textlabel, True, True, 0)

        self.btn_ok = Gtk.Button(label="Copy", halign=Gtk.Align.END)
        self.btn_ok.set_can_default(True)

        box.pack_start(self.btn_ok, True, True, 0)

        self.btn_ok.set_sensitive(False)
        self.btn_ok.connect("clicked", self.copy_text)
        # self.btn_cancel.connect("clicked", Gtk.main_quit)

        self.textentry.set_activates_default(True)
        self.set_default(self.btn_ok)

        self.connect("key-press-event", self.exit_handler)

    def copy_text(self, widget):
        text = self.textentry.get_text()
        self.clipboard.set_text(text, -1)
        self.textlabel.set_text(f"Text copied: {text}")
        self.textentry.set_text("")

    def check_text(self, widget):
        self.btn_ok.set_sensitive(self.textentry.get_text() != "")

    def exit_handler(self, widget, event):
        key = Gdk.keyval_name(event.keyval)
        if event.state & Gdk.ModifierType.CONTROL_MASK and key == "j":
            Gtk.main_quit()


win = ClipboardWindow()
win.connect("destroy", Gtk.main_quit)
win.show_all()
Gtk.main()
