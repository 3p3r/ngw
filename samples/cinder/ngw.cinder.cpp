#include "cinder/app/RendererGl.h"
#include "cinder/gl/GlslProg.h"
#include "cinder/gl/Texture.h"
#include "cinder/app/App.h"
#include "cinder/gl/gl.h"

#include "ngw.hpp"

// simple BGRA to RGBA shader
namespace {
const std::string VERTEX_SHADER = "#version 330 core\n"
    "in vec4 ciPosition;\n"
    "in vec2 ciTexCoord0;\n"
    "uniform mat4 ciModelViewProjection;\n"
    "smooth out vec2 vTexCoord;\n"
    "void main() {\n"
    "  gl_Position = ciModelViewProjection * ciPosition;\n"
    "  vTexCoord = vec2(ciTexCoord0.x, 1.0 - ciTexCoord0.y);\n"
    "}";

const std::string FRAG_SHADER = "#version 330 core\n"
    "uniform sampler2D uTex0;\n"
    "smooth in vec2 vTexCoord;\n"
    "out vec4 fColor;\n"
    "void main()\n"
    "{\n"
    "  fColor = texture( uTex0, vTexCoord ).bgra;\n"
    "}\n";
}

using namespace ci;
using namespace ci::app;
using namespace std;
using namespace ngw;

class NgwCinderApp : public App {
public:
    void                setup() override;
    void                update() override;
    void                draw() override;
    void                keyDown(KeyEvent event) override;
    void                fileDrop(FileDropEvent event) override;
    void                openVideo(const std::string& path);

private:
    class CinderPlayer : public ngw::Player
    {
    protected:

    public:
        gl::TextureRef  mTexture;
        virtual void    onFrame(guchar* buf, gsize size) const override;
        virtual void    onError(const gchar* msg) const override;
        virtual void    onState(GstState old) const override;
        virtual void    onStreamEnd() const override;
    };

private:
    gl::GlslProgRef     mBgra2RgbaShader;
    gl::TextureRef      mTexture;
    CinderPlayer        mPlayer;
};

void NgwCinderApp::CinderPlayer::onFrame(guchar* buf, gsize size) const
{
    if (mTexture)
    {
        gl::ScopedTextureBind tb(mTexture);
        mTexture->update(buf, GL_RGBA, GL_UNSIGNED_BYTE, 0, getWidth(), getHeight());
    }
}

void NgwCinderApp::CinderPlayer::onError(const gchar* msg) const
{
    std::cerr << "Error received from GStreamer: " << msg << std::endl;
}

void NgwCinderApp::CinderPlayer::onStreamEnd() const
{
    std::cout << "EOS event received from GStreamer." << std::endl;
}

void NgwCinderApp::CinderPlayer::onState(GstState old) const
{
    std::cout << "GStreamer state changed from: " << old << " to: " << getState() << std::endl;
}

void NgwCinderApp::setup()
{
    std::cout << "Media Player Usage: Drag-Drop a video to play." << std::endl;
    std::cout << "[p]: play" << std::endl;
    std::cout << "[s]: stop" << std::endl;
    std::cout << "[m]: mute" << std::endl;
    std::cout << "[l]: loop" << std::endl;
    std::cout << "[c]: close" << std::endl;
    std::cout << "[a]: auto-play" << std::endl;
    std::cout << "[f]: full-screen" << std::endl;
    std::cout << "[r]: reverse-playback" << std::endl;

    mBgra2RgbaShader = gl::GlslProg::create(VERTEX_SHADER, FRAG_SHADER);
}

void NgwCinderApp::keyDown(KeyEvent event)
{
    switch (event.getChar())
    {
    case 'p': case 'P':
        mPlayer.getState() == GST_STATE_PLAYING ? mPlayer.pause() : mPlayer.play();
        break;
    case 's': case 'S':
        mPlayer.stop();
        break;
    case 'm': case 'M':
        mPlayer.setMute(!mPlayer.getMute());
        break;
    case 'l': case 'L':
        mPlayer.setLoop(!mPlayer.getLoop());
        break;
    case 'c': case 'C':
        mPlayer.close();
        break;
    case 'f': case 'F':
        setFullScreen(!isFullScreen());
        break;
    case 'r': case 'R':
        mPlayer.setRate(-1. * mPlayer.getRate());
        break;
    default:
        break;
    }

    if (event.getCode() == event.KEY_ESCAPE)
        quit();

    if (event.getCode() == event.KEY_UP) {
        mPlayer.setVolume(mPlayer.getVolume() + .2);
    } else if (event.getCode() == event.KEY_DOWN) {
        mPlayer.setVolume(mPlayer.getVolume() - .2);
    } else if (event.getCode() == event.KEY_LEFT) {
        mPlayer.setTime( mPlayer.getTime() - 5. );
    } else if (event.getCode() == event.KEY_RIGHT) {
        mPlayer.setTime( mPlayer.getTime() + 5. );
    }
}

void NgwCinderApp::fileDrop(FileDropEvent event)
{
    if (event.getNumFiles() > 0) { openVideo(event.getFile(0).generic_string()); }
}

void NgwCinderApp::openVideo(const std::string& path)
{
    mPlayer.mTexture    = nullptr;
    mTexture            = nullptr;

    if (mPlayer.open(path.c_str()))
    {
        mTexture = gl::Texture::create(mPlayer.getWidth(), mPlayer.getHeight());
        mPlayer.mTexture = mTexture;
        mPlayer.play();
    }
}

void NgwCinderApp::update()
{
    mPlayer.update();
}

void NgwCinderApp::draw()
{
    gl::clear();

    if (mBgra2RgbaShader && mTexture)
    {
        gl::ScopedGlslProg      bind(mBgra2RgbaShader);
        gl::ScopedTextureBind   texb(mTexture);
        mBgra2RgbaShader->uniform("uTex0", 0);
        gl::drawSolidRect(getWindowBounds());
    }

    ci::gl::drawString(std::to_string(getAverageFps()), glm::vec2(10, 10));
}

CINDER_APP(NgwCinderApp, RendererGl, [](App::Settings* settings)
{
    settings->setConsoleWindowEnabled();
})
