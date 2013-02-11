CSC = /usr/local/bin/mcs

CSVERSION = future

DEF = LINUX

SRCDIR = src

SRC = \
	$(SRCDIR)/*.cs \
	Properties/*.cs

LIB = OpenTK.dll,System.Drawing.dll,System.Windows.Forms

TARGET = bin/release/ComputerGraphicsCoursework.exe

release:
	mkdir -p bin/release
	rm -f $(TARGET)
	$(CSC) -langversion:$(CSVERSION) $(SRC) -r:$(LIB) -d:$(DEF) \
		-t:exe -out:$(TARGET) -optimize+
	cp OpenTK.dll bin/release/
