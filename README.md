![Dragablz](https://dragablz.files.wordpress.com/2015/01/dragablztext22.png "Dragablz")
========
[![Gitter](https://img.shields.io/badge/Gitter-Join%20Chat-green.svg?style=flat-square)](https://gitter.im/ButchersBoy/Dragablz?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Downloads](https://img.shields.io/nuget/dt/Dragablz.svg?style=flat-square)](http://www.nuget.org/packages/Dragablz/)
[![Build](https://img.shields.io/appveyor/ci/ButchersBoy/dragablz.svg?style=flat-square)](https://ci.appveyor.com/project/ButchersBoy/dragablz)
[![Issues](https://img.shields.io/github/issues/ButchersBoy/MaterialDesignInXamlToolkit.svg?style=flat-square)](https://github.com/ButchersBoy/Dragablz/issues)
[![Twitter](https://img.shields.io/badge/twitter-%40james__willock-55acee.svg?style=flat-square)](https://twitter.com/James_Willock)
## Tearable tab control for WPF, which includes docking, tool windows and MDI.

![Alt text](http://dragablz.files.wordpress.com/2014/11/dragablztearout.gif "Demo shot")

<sup>Illustrates basic theme, more themes at end of page</sup>

- Docs 'n' help 'n' stuff: [dragablz.net](http://dragablz.net/)
- NuGet details here: http://www.nuget.org/packages/Dragablz/
- You can criticise the developer here: [@James_Willock](http://twitter.com/James_Willock) or here: [james@dragablz.net]
  - No, seriously, please get involved and give me a ping with any questions/requests.

## Minimal XAML:

XAML as simple as this will give you a tab the tears out (using the basic theme).  

```xml
<dragablz:TabablzControl Margin="8">
    <dragablz:TabablzControl.InterTabController>
        <dragablz:InterTabController />
    </dragablz:TabablzControl.InterTabController>
    <TabItem Header="Tab No. 1" IsSelected="True">
        <TextBlock>Hello World</TextBlock>
    </TabItem>
    <TabItem Header="Tab No. 2">
        <TextBlock>We Have Tearable Tabs!</TextBlock>
    </TabItem>
</dragablz:TabablzControl>
```
## Features:

- Drag and tear tabs
- User friendly docking
- Floating tool windows & MDI
- Supports MVVM
- IE style transparent Windows
- Fully style-able, included styles:
  - Basic
  - MahApps: [demo](https://github.com/ButchersBoy/DragablzMeetzMahApps) | [theme](https://github.com/ButchersBoy/Dragablz/blob/master/Dragablz/Themes/MahApps.xaml) |  [MahApps](https://github.com/MahApps/MahApps.Metro)
  - Material Design: [tutorial](http://dragablz.net/2015/02/09/how-to-use-the-material-design-theme-with-dragablz-tab-controlgithub/) | [theme](https://github.com/ButchersBoy/Dragablz/blob/master/Dragablz/Themes/MaterialDesign.xaml) |  [guidelines](http://www.google.co.uk/design/spec/style/color.html#color-ui-color-application) | [Material Design in XAML Toolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- Chrome style trapzoid tabs
- Custom (and optional) Window which supports transparency, resizing, snapping, full Window content.
- Miminal XAML required, but hooks provided for advanced control from client code
- Single light weight assembly targeting .net 4.* frameworks, no additional dependencies
- Demos in source (make sure you restore NuGet packages)

## Getting Started:

Here are some helpful blog posts to help you get started:
 - [Getting Started](http://dragablz.net/2014/11/18/getting-started-with-dragablz-tabablzcontrol/)
 - [MDI](http://dragablz.net/2015/01/26/mdi-in-wpf-via-dragablz/)
 - [MahApps Styles](http://dragablz.net/2015/01/06/dragablz-meets-mahapps/)
 - [Material Design 1](http://dragablz.net/2015/02/09/how-to-use-the-material-design-theme-with-dragablz-tab-controlgithub/)
 - [Material Design 2](http://dragablz.net/2015/02/25/material-design-in-xaml-mash-up/)


## In the pipeline:

- Layout persistance and restore
- Extra themes

## Some examples:

Material Design theme (see [Material Design in XAML Toolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)):

![Alt text](https://raw.githubusercontent.com/ButchersBoy/MaterialDesignInXamlToolkit/master/web/images/MashUp.gif "Material Design style")

Docking:

![Alt text](http://dragablz.files.wordpress.com/2014/11/dockablzone1.gif "Docking demo")

MDI:

![Alt text](https://dragablz.files.wordpress.com/2015/01/mdidemo2.gif "MDI demo")

MahApps theme:

![Alt text](https://dragablz.files.wordpress.com/2015/02/mahappsstylez2.gif "MahApps style")


