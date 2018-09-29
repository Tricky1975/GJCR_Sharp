# Information for Linux users

I have only tried this out in Ubuntu, and in Ubuntu I know this works, when it comes to other distros it will very likely come down to substituting "apt" to "yum" or "pacman" or whatever manager your distro provides.

You will very likely see that GJCR comes with a .exe file, which is normally Windows only, however, since GJCR.exe is not a Windows executable but a Mono executable, things are a little bit different.

There are three dependencies you need and below I'll write down what to type in the console to get GJCR.exe running:

~~~shell
sudo apt install mono-runtime
sudo apt install posix.mono
sudo apt install gtk-sharp2
~~~


When this is all working you can just create your database file to set up its structure with your favorite text editor, which is also who the other OSes work in this department and to start MyData you'll just have to do this:
~~~shell
cd <the full path of the folder where you put MyData.exe>
mono GJCR.exe
~~~
That should get everything on the move.



