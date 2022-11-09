const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { VueLoaderPlugin } = require("vue-loader");

const stylesheets = ["./src/styles/index.scss"];
  
var path = require('path')
var webpack = require('webpack')

module.exports = {
  entry: {
    healthcheck: [...stylesheets, './src/entry/index.ts'],
    metrics: './src/entry/others/metrics.ts',
    releaseNotesSummary: './src/entry/others/release-notes-summary.ts',
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
      // Rule: SASS and CSS files from Vue Single File Components
      {
        test: /\.vue\.(s?[ac]ss)$/,
        use: ['vue-style-loader', 'css-loader', 'sass-loader']
      },
      // Rule: SASS and CSS files (standalone)
      {
          test: /(?<!\.vue)\.(s?[ac]ss)$/,
          use: [MiniCssExtractPlugin.loader, 'css-loader', 'sass-loader']
      },
      // // Rule: SASS
      // {
      //   test: /\.(scss|sass)$/,
      //   use: [
      //     {
      //       loader: MiniCssExtractPlugin.loader,
      //     },
      //     "css-loader",
      //     "sass-loader",
      //   ],
      // },
      // // Rule: CSS
      // {
      //   test: /\.css$/,
      //   use: [
      //     {
      //       loader: MiniCssExtractPlugin.loader,
      //       options: { hmr: !isProduction }
      //     },
      //     "style-loader",
      //     "css-loader",
      //   ],
      // },
      // Rule: VUE
      {
        test: /\.vue$/,
        loader: 'vue-loader',
        // options: {
        //   loaders: {
        //     // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
        //     // the "scss" and "sass" values for the lang attribute to the right configs here.
        //     // other preprocessors should work out of the box, no loader config like this necessary.
        //     'scss': 'vue-style-loader!css-loader!sass-loader',
        //     'sass': 'vue-style-loader!css-loader!sass-loader',
        //   }
        // }
      },
      // Rule: TSX
      {
        test: /\.tsx?$/,
        loader: 'ts-loader',
        exclude: /node_modules/,
        options: {
          appendTsSuffixTo: [/\.vue$/],
        }
      },
      // Rule: FILES
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
    extensions: ['.ts', '.js', '.vue', '.json', '.scss'],
    alias: {
      'vue$': 'vue/dist/vue.runtime.esm-bundler.js',
      "@generated": path.resolve(__dirname, "./src/generated"),
      "@components": path.resolve(__dirname, "./src/components"),
      "@models": path.resolve(__dirname, "./src/models"),
      "@services": path.resolve(__dirname, "./src/services"),
      "@systems": path.resolve(__dirname, "./src/systems"),
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
    PRODMODE: JSON.stringify(isProd),
    __VUE_OPTIONS_API__: true,
    __VUE_PROD_DEVTOOLS__: false
  }),
  new webpack.optimize.LimitChunkCountPlugin({
    maxChunks: 1
  }),
  new MonacoWebpackPlugin({
    languages: [ 'csharp', 'json', 'xml', 'sql' ],
    // filename: '[name].worker.js'
    publicPath: '/'
  }),
  new MiniCssExtractPlugin()
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