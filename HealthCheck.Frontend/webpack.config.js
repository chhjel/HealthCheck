const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const { VueLoaderPlugin } = require("vue-loader");
  
var path = require('path')
var webpack = require('webpack')

module.exports = {
  entry: {
    healthcheck: './src/entry/index.ts',
    metrics: './src/entry/others/metrics.ts',
    releaseNotesSummary: './src/entry/others/release-notes-summary.ts'
  },
  output: {
    path: path.resolve(__dirname, './dist'),
    publicPath: '/dist/',
    filename: '[name].js',
    chunkFilename: '[name].vendor.[id].js'
  },
  optimization: {
    splitChunks: {
      chunks: 'async'
    }
  },
  module: {
    rules: [
      {
        test: /\.s(c|a)ss$/,
        use: [
          'vue-style-loader',
          'css-loader',
          {
            loader: 'sass-loader',
            options: {
              implementation: require('sass'),
              sassOptions: {
                fiber: require('fibers'),
                // indentedSyntax: true // optional
              },
            }
          }
        ]
      },
      {
        test: /\.css$/,
        use: ['style-loader', 'css-loader'],
      },
      {
        test: /\.vue$/,
        loader: 'vue-loader',
        options: {
          loaders: {
            // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
            // the "scss" and "sass" values for the lang attribute to the right configs here.
            // other preprocessors should work out of the box, no loader config like this necessary.
            'scss': 'vue-style-loader!css-loader!sass-loader',
            'sass': 'vue-style-loader!css-loader!sass-loader',
          }
          // other vue-loader options go here
        }
      },
      {
        test: /\.tsx?$/,
        loader: 'ts-loader',
        exclude: /node_modules/,
        options: {
          appendTsSuffixTo: [/\.vue$/],
        }
      },
      {
        test: /\.(png|jpg|gif|svg|ttf)$/,
        loader: 'file-loader',
        options: {
          name: '[name].[ext]?[hash]'
        }
      }
    ]
  },
  resolve: {
    extensions: ['.ts', '.js', '.vue', '.json'],
    alias: {
      'vue$': 'vue/dist/vue.runtime.esm-bundler.js',
      "@generated": path.resolve(__dirname, "./src/generated"),
      "@components": path.resolve(__dirname, "./src/components"),
      "@models": path.resolve(__dirname, "./src/models"),
      "@services": path.resolve(__dirname, "./src/services"),
      "@util": path.resolve(__dirname, "./src/util"),
      "src": path.resolve(__dirname, "./src")
    }
  },
  devServer: {
    historyApiFallback: true,
    noInfo: true
  },
  performance: {
    hints: false
  },
  devtool: 'source-map'
}

const isProd = process.env.NODE_ENV === 'production';
module.exports.plugins = [
  new VueLoaderPlugin(),
  new webpack.DefinePlugin({
    DEVMODE: JSON.stringify(!isProd),
    PRODMODE: JSON.stringify(isProd)
  }),
  new webpack.optimize.LimitChunkCountPlugin({
    maxChunks: 1
  }),
  new MonacoWebpackPlugin({
    languages: [ 'csharp', 'json', 'xml' ],
    // filename: '[name].worker.js'
    publicPath: '/'
  })
  // ,new BundleAnalyzerPlugin()
];

if (process.env.NODE_ENV === 'production') {
  module.exports.devtool = false //'#source-map'
  // http://vue-loader.vuejs.org/en/workflow/production.html
  module.exports.plugins = (module.exports.plugins || []).concat([
    new webpack.DefinePlugin({
      'process.env': {
        NODE_ENV: '"production"'
      }
    }),
    new webpack.LoaderOptionsPlugin({
      minimize: true
    })
  ])
}