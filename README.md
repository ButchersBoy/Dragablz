![Dragablz](https://dragablz.files.wordpress.com/2015/01/dragablztext22.png "Dragablz")
========
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/ButchersBoy/Dragablz?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Downloads](https://img.shields.io/nuget/dt/Dragablz.svg)](http://www.nuget.org/packages/Dragablz/)
[![Build](https://ci.appveyor.com/api/projects/status/7ckfd53w938q3iaa?svg=true)](https://ci.appveyor.com/project/ButchersBoy/dragablz)
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
  - MahApps: [demo](https://github.com/ButchersBoy/DragablzMeetzMahApps]) | [theme](https://github.com/ButchersBoy/Dragablz/blob/master/Dragablz/Themes/MahApps.xaml) |  [MahApps](https://github.com/MahApps/MahApps.Metro)
  - Material Design [tutorial](http://dragablz.net/2015/02/09/how-to-use-the-material-design-theme-with-dragablz-tab-controlgithub/) | [theme](https://github.com/ButchersBoy/Dragablz/blob/master/Dragablz/Themes/MaterialDesign.xaml) |  [guidelines](http://www.google.co.uk/design/spec/style/color.html#color-ui-color-application)
- Chrome style trapzoid tabs
- Custom (and optional) Window which supports transparency, resizing, snapping, full Window content.
- Miminal XAML required, but hooks provided for advanced control from client code
- Single light weight assembly targeting .net 4.* frameworks, no additional dependencies
- Demos in source (make sure you restore NuGet packages)

## In the pipeline:

- Layout persistance and restore
- Extra themes

## Some examples:

Material Design theme:

![Alt text](https://dragablz.files.wordpress.com/2015/02/materialdesigndemo1.gif "Material Design style")

Docking:

![Alt text](http://dragablz.files.wordpress.com/2014/11/dockablzone1.gif "Docking demo")

MDI:

![Alt text](https://dragablz.files.wordpress.com/2015/01/mdidemo2.gif "MDI demo")

MahApps theme:

![Alt text](https://dragablz.files.wordpress.com/2015/02/mahappsstylez2.gif "MahApps style")


```
________                            ___.   .__          
\______ \____________     _________ \_ |__ |  | ________
 |    |  \_  __ \__  \   / ___\__  \ | __ \|  | \___   /
 |    `   \  | \// __ \_/ /_/  > __ \| \_\ \  |__/    / 
/_______  /__|  (____  /\___  (____  /___  /____/_____ \
        \/           \//_____/     \/    \/           \/
```
