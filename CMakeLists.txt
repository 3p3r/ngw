CMAKE_MINIMUM_REQUIRED( VERSION 3.0 FATAL_ERROR )

# //////////////////////////////////////////////////////////////////////
# Project-wide settings:
PROJECT( ngw )

SET( NGW_ROOT
 "${CMAKE_CURRENT_SOURCE_DIR}"
 CACHE PATH "root directory" )

SET( NGW_BINDINGS
 "${NGW_ROOT}/bindings"
 CACHE PATH "bindings directory" )

SET( CMAKE_MODULE_PATH
 ${CMAKE_MODULE_PATH}
 "${NGW_ROOT}/tools/share/cmake" )

#turn on using solution folders
SET_PROPERTY( GLOBAL PROPERTY USE_FOLDERS ON)

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
