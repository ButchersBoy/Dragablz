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
- If you like it **_star_** it, a little love goes a long way and keeps me motivated!

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

## A note on the demo project:

- You will have to restore NuGet packages
- I've seen Visual Studio get confused with both the .net 4.0 and 4.5 projects.  If you see this compile Dragablz.net4*, and then DragablzDemo.net4* by right clicking on the project in the solution explorer.  I'll try and improve this at some point...
- Are you using **MahApps**?  If so then check [this demo project](https://github.com/ButchersBoy/DragablzMeetzMahApps) showing Dragablz and MahApps working together.

## Features:

- Drag and tear tabs
- User friendly docking
- Floating tool windows & MDI
- Supports MVVM
- IE style transparent Windows
- Styles included to work with [MahApps](https://github.com/MahApps/MahApps.Metro)
- Chrome style trapzoid tabs
- Custom (and optional) Window which supports transparency, resizing, snapping, full Window content.
- Miminal XAML required, but hooks provided for advanced control from client code
- Single light weight assembly targeting .net 4.* frameworks, no additional dependencies

## In the pipeline:

- Layout persistance and restore
- Extra themes

## Some more examples:

![Alt text](http://dragablz.files.wordpress.com/2014/11/dockablzone1.gif "Docking demo")
<sup>Docking demo</sup>

![Alt text](https://dragablz.files.wordpress.com/2015/01/mdidemo2.gif "MDI demo")
<sup>MDI demo</sup>

![Alt text](https://dragablz.files.wordpress.com/2015/02/mahappsstylez2.gif "MahApps style")
<sup>MahApps theme</sup>

![Alt text](https://dragablz.files.wordpress.com/2015/02/materialdesigndemo1.gif "Material Design style")
<sup>Google Material Design theme</sup>

```
________                            ___.   .__          
\______ \____________     _________ \_ |__ |  | ________
 |    |  \_  __ \__  \   / ___\__  \ | __ \|  | \___   /
 |    `   \  | \// __ \_/ /_/  > __ \| \_\ \  |__/    / 
/_______  /__|  (____  /\___  (____  /___  /____/_____ \
        \/           \//_____/     \/    \/           \/
```
