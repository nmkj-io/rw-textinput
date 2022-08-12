__version__ = "0.1.0"

import sys
from PySide2 import QtWidgets, QtCore

class MyWidget(QtWidgets.QWidget):
    def __init__(self, app: QtWidgets.QApplication):
        super().__init__()

        self.app = app
        self.layout = QtWidgets.QVBoxLayout(self)
        self.input = QtWidgets.QLineEdit()
        self.label = QtWidgets.QLabel("Input text here:", alignment=QtCore.Qt.AlignLeft)
        self.button = QtWidgets.QPushButton("[Ctrl-J] Copy")
        self.button.setAutoDefault(False)
        self.button.setDefault(False)

        self.layout.addWidget(self.label)
        self.layout.addWidget(self.input)
        self.layout.addWidget(self.button)
        self.setLayout(self.layout)

        self.button.setShortcut(self.tr("Ctrl+J"))
        self.button.clicked.connect(self.clicked)

    @QtCore.Slot()
    def clicked(self):
        print(self.input.text(), end="")
        self.app.exit(0)

def main() -> None:
    # retcode = 1
    app = QtWidgets.QApplication(sys.argv)
    # text, ok = QtWidgets.QInputDialog().getText(None, "RimWorld Text Input Helper", "Input text here:")

    # if ok and text:
    #     print(text, end="")
    #     retcode = 0

    widget = MyWidget(app)
    widget.setWindowTitle("RimWorld Input Helper")
    widget.show()

    sys.exit(app.exec_())
