# //////////////////////////////////////////////////////////////////////
# Finding GStreamer 1.0 SDK root directory
# NOTE: based on the architecture, GST defines different env vars.

# Windows systems
IF (MSVC)

# CPU Architecture detection
INCLUDE( DetectTargetArch REQUIRED )
DETECT_TARGET_ARCH( ARCH )

# GStreamer environment variable detection
IF ( ARCH STREQUAL "i386" )
    SET ( GST_ENV_VAR_NAME "GSTREAMER_1_0_ROOT_X86" )
ELSEIF( ARCH STREQUAL "x86_64" )
    SET ( GST_ENV_VAR_NAME "GSTREAMER_1_0_ROOT_X86_64" )
ELSE() # bad / unknown arch
    MESSAGE( FATAL_ERROR "Bad arch (${ARCH}) passed to FindGstreamer" )
ENDIF()

FIND_PATH( GSTREAMER_ROOT_DIR "gst.h"
    PATHS ENV ${GST_ENV_VAR_NAME}
    PATH_SUFFIXES
    "include/gstreamer-1.0/gst"
    DOC "Path to the root of GStreamer SDK"
    NO_DEFAULT_PATH )

FIND_PATH( GSTREAMER_PROPS_DIR "gstreamer-1.0.props"
    PATHS ENV ${GST_ENV_VAR_NAME}
    PATH_SUFFIXES
    "share/vs/2010/libs"
    DOC "Path to the property sheets of GStreamer SDK"
    NO_DEFAULT_PATH )

# In case we cannot find GST, its variable should be equal to ""
IF( NOT GSTREAMER_ROOT_DIR OR NOT GSTREAMER_PROPS_DIR )

    # Try one last time with GSTREAMER_ROOT variable
    FIND_PATH( GSTREAMER_ROOT_DIR "gst.h"
        PATHS ENV GSTREAMER_ROOT
        PATH_SUFFIXES
        "include/gstreamer-1.0/gst"
        DOC "Path to the root of GStreamer SDK"
        NO_DEFAULT_PATH )

    FIND_PATH( GSTREAMER_PROPS_DIR "gstreamer-1.0.props"
        PATHS ENV GSTREAMER_ROOT
        PATH_SUFFIXES
        "share/vs/2010/libs"
        DOC "Path to the property sheets of GStreamer SDK"
        NO_DEFAULT_PATH )

    IF( NOT GSTREAMER_ROOT_DIR OR NOT GSTREAMER_PROPS_DIR )
        # Die if we cannot find GST even after trying GSTREAMER_ROOT
        MESSAGE( FATAL_ERROR "Could not find GStreamer SDK. Make sure "
        "you have the SDK installed and environment variables defined "
        "Either GSTREAMER_ROOT or GSTREAMER_1_0_ROOT_XXX" )
    ENDIF()

ENDIF()

# we don't do anything else beyond this point. Rest is handled by all
# property sheets that come with GST dev sdk. CMake will post-process
# the project file and inject them there.

MACRO( TARGET_ADD_GSTREAMER_MODULES target_name )
    SET( DOT_USER_CONTENT "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" )
    SET( DOT_USER_CONTENT "${DOT_USER_CONTENT}<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n" )
    FOREACH(arg ${ARGN})
        GET_FILENAME_COMPONENT( PROP_PATH "${GSTREAMER_PROPS_DIR}/${arg}.props" ABSOLUTE )
        IF ( EXISTS ${PROP_PATH} )
            SET( DOT_USER_CONTENT "${DOT_USER_CONTENT}<Import Project=\"${PROP_PATH}\" />\n" )
        ELSE(  )
            MESSAGE( "Requested GST module ${arg} does not have a property sheet." )
        ENDIF()
    ENDFOREACH()
    SET( DOT_USER_CONTENT "${DOT_USER_CONTENT}</Project>" )
    FILE( WRITE "${CMAKE_CURRENT_BINARY_DIR}/${target_name}.vcxproj.user" "${DOT_USER_CONTENT}" )
    SET( DOT_USER_CONTENT )
ENDMACRO()

# Linux systems
ELSEIF(UNIX AND NOT CYGWIN AND NOT APPLE)

FIND_PACKAGE(PkgConfig)

MACRO(TARGET_ADD_GSTREAMER_MODULES target_name)
    FOREACH(arg ${ARGN})
        STRING(REPLACE "-" ";" comps ${arg})
        LIST(LENGTH comps comps_length)
        SET(_library "")
        IF (${comps_length} GREATER 2)
            LIST(GET comps 1 comp)
            SET(_library "gst${comp}-1.0")
        ELSE()
            SET(_library "${arg}")
        ENDIF()
        
        STRING(REPLACE "-" "_" _component_prefix ${arg})
        STRING(TOUPPER ${_component_prefix} _component_prefix)
        STRING(REPLACE "_1.0" "" _component_prefix ${_component_prefix})
        
        PKG_CHECK_MODULES(PC_${_component_prefix} ${arg})
        TARGET_INCLUDE_DIRECTORIES(${target_name} PUBLIC ${PC_${_component_prefix}_INCLUDE_DIRS})

        FIND_LIBRARY(${_component_prefix}_LIBRARIES
            NAMES ${_library}
            HINTS ${PC_${_component_prefix}_LIBRARY_DIRS} ${PC_${_component_prefix}_LIBDIR})
        TARGET_LINK_LIBRARIES(${target_name} ${${_component_prefix}_LIBRARIES})
    ENDFOREACH()
ENDMACRO()

ENDIF()