package main

import (
	"fmt"
	"log"
	"os"

	"github.com/gotk3/gotk3/gtk"
)

func main() {
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
	entry.SetActivatesDefault(true)

	btn, err := gtk.ButtonNewWithLabel("Copy")
	if err != nil {
		log.Fatal("Unable to create button:", err)
	}

	btn.SetHAlign(gtk.ALIGN_END)
	btn.SetCanDefault(true)
	btn.SetSensitive(false)

	btn.Connect("clicked", func() {
		text, err := entry.GetText()
		if err != nil {
			log.Fatal("Unable to get text:", err)
		}
		fmt.Print(text)
		exitCode = 0
		gtk.MainQuit()
	})
	entry.Connect("changed", func() {
		text, err := entry.GetText()
		if err != nil {
			log.Fatal("Unable to get text:", err)
		}
		btn.SetSensitive(text != "")
	})
	win.SetDefault(btn)

	box, err := gtk.BoxNew(gtk.ORIENTATION_VERTICAL, 2)
	if err != nil {
		log.Fatal("Unable to create box:", err)
	}
	box.PackStart(label, true, true, 0)
	box.PackStart(entry, true, true, 0)
	box.PackStart(btn, true, true, 0)

	// Add the label to the window.
	win.Add(box)

	// Recursively show all widgets contained in this window.
	win.ShowAll()

	// Begin executing the GTK main loop.  This blocks until
	// gtk.MainQuit() is run.
	gtk.Main()

	os.Exit(exitCode)
}
