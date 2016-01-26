var gulp = require('gulp'),
    del = require('del'),
    sass = require('gulp-ruby-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    minifycss = require('gulp-minify-css'),
    jshint = require('gulp-jshint'),
    uglify = require('gulp-uglify'),
    imagemin = require('gulp-imagemin'),
    rename = require('gulp-rename'),
    concat = require('gulp-concat'),
    notify = require('gulp-notify'),
    cache = require('gulp-cache'),
    plumber = require('gulp-plumber'),
    cmq = require('gulp-combine-media-queries');






gulp.task('css', function() {
  return sass('dev/scss/main.scss', { style: 'expanded' })
    .pipe(plumber())
    .pipe(autoprefixer('last 2 version'))
    .pipe(gulp.dest('assets/css'))
    .pipe(rename({suffix: '.min'}))
    .pipe(cmq({
      log: true
    }))
    .pipe(minifycss())
    .pipe(gulp.dest('assets/css'))
    // .pipe(notify({ message: 'css task complete' }));
});


gulp.task('js', function() {
  return gulp.src('dev/js/*.js')
    .pipe(plumber())
    // .pipe(jshint('.jshintrc'))
    .pipe(jshint.reporter('default'))
    .pipe(concat('main.js'))
    .pipe(gulp.dest('assets/js'))
    .pipe(rename({suffix: '.min'}))
    .pipe(uglify())
    .pipe(gulp.dest('assets/js'))
    // .pipe(notify({ message: 'js task complete' }));
});


gulp.task('img', function() {
  return gulp.src('dev/img/*')
    .pipe(imagemin({ optimizationLevel: 3, progressive: true, interlaced: true }))
    // .pipe(cache(imagemin({ optimizationLevel: 5, progressive: true, interlaced: true })))
    .pipe(gulp.dest('assets/img'))
    // .pipe(notify({ message: 'img task complete' }));
});




gulp.task('clean', function() {
    del(['assets/css', 'assets/js', 'assets/img'])
});




gulp.task('default', ['clean'], function() {
    gulp.start('css', 'js', 'img');
});

gulp.task('watch', function() {

  // Watch .scss files
  gulp.watch('dev/scss/**/*', ['css']);

  // Watch .js files
  gulp.watch('dev/js/*.js', ['js']);

  // Watch image files
  gulp.watch('dev/img/*', ['img']);





});

