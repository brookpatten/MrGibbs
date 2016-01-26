![Ghost Compatability](http://img.shields.io/badge/Compatible%20with%20Ghost-v0.7.x-brightgreen.svg)

# Escritor - a theme for [Ghost](http://github.com/tryghost/ghost)

**[DEMO](http://escritor.ozywuli.com)**

Escritor is a modern and elegant theme for sharing your thoughts, stories, and ideas.

If you find any bugs, want to request any features, or have any other advice for improving this theme, please open an issue or make a pull request. Your help is much appreciated!

Key Features

- Disqus comments
- Prism.js syntax highlighting for HTML, CSS, and Javascript
- Responsive video embeds
- Archive page
- Recent articles in sidebar


# INSTALLATION AND CUSTOMIZATION

You'll need node, npm, and gulp installed

+ http://nodejs.org/
+ https://www.npmjs.com/
+ http://gulpjs.com

After which you should follow instructions below:

Clone the repository into the themes folder located in your local ghost installation
```bash
git clone https://github.com/ozywuli/escritor-ghost-theme
```

Navigate to the cloned repository
```bash
cd escritor-ghost-theme
```
Install the necessary gulp plugins
```bash
npm install del gulp-ruby-sass gulp-autoprefixer gulp-minify-css gulp-jshint gulp-uglify gulp-imagemin gulp-rename gulp-concat gulp-notify gulp-cache gulp-livereload del gulp-plumber gulp-combine-media-queries --save-dev
```
Build the theme
```bash
gulp
```

Gulp will build all the necessary assets located in the dev directory and place them into the build directory. To watch for changes, use the following command:

```bash
gulp watch
```

If you have any questions, please don't hesitate to ask!

Supports the latest version of Ghost on IE10+, Firefox, Chrome, Safari