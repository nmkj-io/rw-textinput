package main

import (
	"flag"
	"fmt"
	"log"
	"os"
	"strings"

	"github.com/gotk3/gotk3/gdk"
	"github.com/gotk3/gotk3/gtk"
)

var winWidth = flag.Int("w", -1, "width")
var winHeight = flag.Int("h", -1, "height")
var defaultText = flag.String("d", "", "default string")
var textIsMultiline = flag.Bool("m", false, "multiline")

func main() {
	flag.Parse()

	// Initialize GTK without parsing any command line arguments.
	gtk.Init(nil)
	exitCode := 1

	// Create a new toplevel window, set its title, and connect it to the
	// "destroy" signal to exit the GTK main loop when it is destroyed.
	win, err := gtk.WindowNew(gtk.WINDOW_TOPLEVEL)
	if err != nil {
		log.Fatal("Unable to create window:", err)
	}
	win.SetTitle("RimWorld Text Input")
	win.Connect("destroy", func() {
		gtk.MainQuit()
	})

	// Create a new label widget to show in the window.
	label, err := gtk.LabelNew("Input your text")
	label.SetHAlign(gtk.ALIGN_START)
	if err != nil {
		log.Fatal("Unable to create label:", err)
	}
	entry, err := gtk.EntryNew()
	if err != nil {
		log.Fatal("Unable to create entry:", err)
	}
	entryMulti, err := gtk.TextViewNew()
	if err != nil {
		log.Fatal("Unable to create textview:", err)
	}
	entry.SetActivatesDefault(true)
	// entryMulti.SetWrapMode(gtk.WRAP_CHAR)
	entryMulti.SetBorderWidth(5)

	btn, err := gtk.ButtonNewWithLabel("Copy")
	if err != nil {
		log.Fatal("Unable to create button:", err)
	}
	cancelBtn, err := gtk.ButtonNewWithLabel("Cancel")
	if err != nil {
		log.Fatal("Unable to create button:", err)
	}

	btn.SetHAlign(gtk.ALIGN_END)
	btn.SetCanDefault(true)
	btn.SetSensitive(false)
	cancelBtn.SetHAlign(gtk.ALIGN_START)
	cancelBtn.SetCanDefault(false)

	cancelBtn.Connect("clicked", func() {
		gtk.MainQuit()
	})
	entry.Connect("changed", func() {
		text, err := entry.GetText()
		if err != nil {
			log.Fatal("Unable to get text:", err)
		}
		btn.SetSensitive(text != "")
	})
	buf, err := entryMulti.GetBuffer()
	if err != nil {
		log.Fatal("Unable to get text buffer:", err)
	}
	if *defaultText != "" {
		*defaultText = strings.ReplaceAll(*defaultText, "\\n", "\n")
		entry.SetText(*defaultText)
		buf.SetText(*defaultText)
	}
	buf.Connect("changed", func() {
		text, err := buf.GetText(buf.GetStartIter(), buf.GetEndIter(), true)
		if err != nil {
			log.Fatal("Unable to get text:", err)
		}
		btn.SetSensitive(text != "")
	})
	btn.Connect("clicked", func() {
		var (
			text string
			err  error
		)
		if *textIsMultiline {
			text, err = buf.GetText(buf.GetStartIter(), buf.GetEndIter(), true)
		} else {
			text, err = entry.GetText()
		}
		if err != nil {
			log.Fatal("Unable to get text:", err)
		}
		text = strings.ReplaceAll(text, "\\n", "\n")
		fmt.Print(text)
		exitCode = 0
		gtk.MainQuit()
	})
	if !*textIsMultiline {
		win.SetDefault(btn)
	}

	box, err := gtk.BoxNew(gtk.ORIENTATION_VERTICAL, 4)
	if err != nil {
		log.Fatal("Unable to create box:", err)
	}
	boxBtn, err := gtk.BoxNew(gtk.ORIENTATION_HORIZONTAL, 0)
	if err != nil {
		log.Fatal("Unable to create box:", err)
	}
	box.PackStart(label, true, true, 0)
	if *textIsMultiline {
		box.PackStart(entryMulti, true, true, 0)
	} else {
		box.PackStart(entry, true, true, 0)
	}
	boxBtn.PackStart(cancelBtn, true, true, 0)
	boxBtn.PackStart(btn, true, true, 0)
	box.PackStart(boxBtn, true, true, 0)

	// Add the label to the window.
	win.SetBorderWidth(4)
	win.SetResizable(true)
	win.Connect("key-press-event", func(_ *gtk.Window, event *gdk.Event) {
		key := gdk.EventKeyNewFromEvent(event)
		if key.KeyVal() == gdk.KEY_Escape {
			gtk.MainQuit()
		}
		mask := gtk.AcceleratorGetDefaultModMask()
		if *textIsMultiline && (key.State()&uint(mask) == gdk.CONTROL_MASK) && key.KeyVal() == gdk.KEY_Return {
			btn.Activate()
		}
	})
	win.Add(box)

	win.SetKeepAbove(true)
	win.SetDefaultSize(*winHeight, *winWidth)

	// Recursively show all widgets contained in this window.
	win.ShowAll()

	// Begin executing the GTK main loop.  This blocks until
	// gtk.MainQuit() is run.
	gtk.Main()

	os.Exit(exitCode)
}
