Nano GStreamer Wrapper
======================

Cross-platform high performance embed-able media playback solution using GStreamer.

---

Nano GStreamer Wrapper or **ngw** in short, is a minimal C++11 wrapper around GStreamer's common functionality to bring media playback to *game engines and interactive frameworks*. At its surface the API is extremely simple and at its core, design of it is *single-threaded and lock-free*; suitable to be embedded in other engines conveniently without worrying about threading issues and synchronization.

This library has two flavors: **ABI stable** and **ABI unstable**. There are different use cases for both. The ABI stable flavor slightly extends ABI unstable's API to provide virtually the same flexibility when embedded.

This library consists of two core classes: `Player` and `Discoverer`.

`Player` class is designed to play any type of media (audio/video) from an absolute local path or a network URL. `Player` class plays back the audio directly from system's default sound output and hands video frames to the user of the library.

`Discoverer` class is used to gather meta data information about a media file without playing it back (such as video frame rate, video dimension, audio sample rate, and etc.). `Player` class uses `Discoverer` internally to gather information such as duration and dimension of the media file before opening it.

It is recommended to use the ABI unstable flavor if you are working with a C++ framework such as *Cinder* or *OpenFrameworks*. ABI stable is recommended to be used inside more mature engines such as *Unity3D* or *Unreal Engine 4*. There are samples inside `samples/` folder to demonstrate mentioned usages.

---

##Install instructions

Regardless of your platform, GStreamer runtime must be installed before using this library. Below are some platform-specific instructions to do so.

###Windows

  * **Runtime**
    
    1. Grab the latest Windows [run time binary](https://gstreamer.freedesktop.org/data/pkg/windows/) installer package from GStreamer project's website and install it.
      * **NOTE:** Make sure you perform a "Complete" installation and not "Typical" to gain access all plugins and codecs. You may do so by clicking on "Complete" option in "Choose Setup Type" screen.
    2. Add GStreamer to User's PATH (notice it is **not system** PATH):
      * For 64 bit systems:
        1. Append `%GSTREAMER_1_0_ROOT_X86_64%\bin` to **user's** PATH variable (create one if does not exist).
        2. Define `GST_PLUGIN_PATH` to `%GSTREAMER_1_0_ROOT_X86_64%\lib\gstreamer-1.0`
      * For 32 bit systems:
        1. Append `%GSTREAMER_1_0_ROOT_X86%\bin` to **user's** PATH variable (create one if does not exist).
        2. Define `GST_PLUGIN_PATH` to `%GSTREAMER_1_0_ROOT_X86%\lib\gstreamer-1.0`
    3. Restart your machine (so environment variables properly populate the system).

  * **Development**
    
    Repeat *Runtime* install instruction above, but this time with the development installer package. *Make sure you install the package exactly where you installed the runtime package.*

###Linux

Installing GStreamer is highly distribution dependent. If you are willing to compile this library under Linux, chances that you know what you are doing is high and you already know how to obtain *gstreamer-1.0* from your package manager. As an example, following instructions are applicable to *Ubuntu 14*:

  * **Runtime**

    ```BASH
    sudo apt-get update && sudo apt-get upgrade
    sudo add-apt-repository ppa:gstreamer-developers/ppa
    sudo apt-get update
    sudo apt-get install gstreamer1.0*
    ```

  * **Development**
    
    ```BASH
    sudo apt-get install build-essential
    sudo apt-get install cmake
    ```

###Mac OSX

  * **Runtime**
    
    GStreamer can be installed by [Homebrew](http://brew.sh/):
    
    ```BASH
    brew install gstreamer
    brew install gst-plugins-base gst-plugins-bad gst-plugins-ugly gst-plugins-good
    brew install gst-libav
    ```
  * **Development**
    
    *XCode* must be installed. Building this library requires *cmake* and *pkgconfig*:
    
    ```BASH
    brew install cmake pkgconfig
    ```

##Install verification

After following the instruction above, you may verify your installation by executing `gst-launch-1.0 --version` in a terminal window. It should output something like:
```
gst-launch-1.0 version 1.7.2
GStreamer 1.7.2
...
```

##Build instructions

Regardless of your platform you need to follow "Install instructions" above first. After that you need [CMake](https://cmake.org/) 2.8+ installed. Building with CMake could be as simple as:

```Bash
cd <ngw root>
mkdir build
cd build
cmake ..
cmake --build . --config Release
```

`ngw.cpp` and `ngw.hpp` files are also portable. You can build them as a part of your source-tree. You need to link against GStreamer independently then.

##API documentation

Documentation is provided in Doxygen format. You can generate the documentation with provided `tools/doxygen/Doxyfile` config file and it outputs to `docs/html`. You may start browsing the documentation by opening up `docs/html/index.html` in a web browser.

##License

Nano GStreamer Wrapper is licensed under LGPLv3 according to advice given at "[Licensing Advice](https://gstreamer.freedesktop.org/documentation/licensing.html)" page available at GStreamer project's website . Additional information can be found in `LICENSE` file.
