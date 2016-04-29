CMAKE_MINIMUM_REQUIRED( VERSION 2.8 FATAL_ERROR )

# //////////////////////////////////////////////////////////////////////
# Project-wide settings:
PROJECT( ngw )

SET( NGW_ROOT
 "${CMAKE_CURRENT_SOURCE_DIR}"
 CACHE PATH "root directory" )

SET( NGW_BINDINGS
 "${NGW_ROOT}/bindings"
 CACHE PATH "bindings directory" )

SET( NGW_SAMPLES
 "${NGW_ROOT}/samples"
 CACHE PATH "samples directory" )

SET( CMAKE_MODULE_PATH
 ${CMAKE_MODULE_PATH}
 "${NGW_ROOT}/tools/share/cmake" )

# turn on using solution folders
SET_PROPERTY( GLOBAL PROPERTY USE_FOLDERS ON)

# turn on C++11 for GCC
IF(CMAKE_COMPILER_IS_GNUCXX)
 LIST(APPEND CMAKE_CXX_FLAGS "-std=c++11 ${CMAKE_CXX_FLAGS}")
ENDIF()

# silence a cmake warning for MAC OSX
IF(APPLE)
 SET(CMAKE_MACOSX_RPATH 1)
ENDIF()

INCLUDE ( FindGstreamer REQUIRED )
INCLUDE_DIRECTORIES( ${NGW_ROOT} )

# main sources:
SET( NGW_HEADER "${NGW_ROOT}/ngw.hpp" )
SET( NGW_SOURCE "${NGW_ROOT}/ngw.cpp" )

# main C++ static library target
ADD_LIBRARY( ngw.static STATIC ${NGW_HEADER} ${NGW_SOURCE} )
TARGET_ADD_GSTREAMER_MODULES( ngw.static
	gstreamer-1.0
	gstreamer-app-1.0
	gstreamer-pbutils-1.0 )

# C bindings shared library target
ADD_LIBRARY( ngw SHARED ${NGW_BINDINGS}/ngw.h ${NGW_BINDINGS}/ngw.h.cpp )
TARGET_ADD_GSTREAMER_MODULES( ngw
	gstreamer-1.0
	gstreamer-app-1.0
	gstreamer-pbutils-1.0 )
TARGET_LINK_LIBRARIES( ngw ngw.static )
TARGET_COMPILE_DEFINITIONS( ngw PUBLIC -DNGW_BUILD_DLL )

FIND_PACKAGE( Cinder QUIET )
IF( Cinder_FOUND )
ADD_EXECUTABLE( ngw.cinder ${NGW_SAMPLES}/cinder/ngw.cinder.cpp )
TARGET_ADD_GSTREAMER_MODULES( ngw.cinder
	gstreamer-1.0
	gstreamer-app-1.0
	gstreamer-pbutils-1.0 )
CONFIGURE_CINDER_TARGET( ngw.cinder )
TARGET_LINK_LIBRARIES( ngw.cinder ngw.static )
ENDIF()

FIND_PACKAGE(OpenGL REQUIRED)
INCLUDE_DIRECTORIES(${OpenGL_INCLUDE_DIRS})
LINK_DIRECTORIES(${OpenGL_LIBRARY_DIRS})
ADD_DEFINITIONS(${OpenGL_DEFINITIONS})
TARGET_LINK_LIBRARIES(ngw ${OPENGL_LIBRARIES})

SET_PROPERTY(TARGET ngw.static ngw PROPERTY CXX_STANDARD 11)
SET_PROPERTY(TARGET ngw.static ngw PROPERTY CXX_STANDARD_REQUIRED ON)

