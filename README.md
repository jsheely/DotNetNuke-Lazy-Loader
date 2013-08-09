DotNetNuke-Lazy-Loader
======================

DotNetNuke Image, File, and Content Lazy Loader

### Work It Works
- Takes the rendered page and replaces all the images with the default 1 pixel gif and adds the attribute for the original image path. Then a client javascript will replace the 1 pixel gif src with the actual image path based when that image is in the viewable screen.
- Takes the rendered fully rendered page before it's sent to the requester and omits any module section. That module section will be requested on the client when that section is in the viewable area. (Example footer)
- If requester is a search engine. All features are disabled so page renders normally.

### Use Cases
- Limiting the bandwidth being used per page view
- Peceived performance by staggering images and module downloads.
- Load module sections when user scrolls down to that section. Like the footer.
- Great for mobile for the above reasons.

### Installation & Configuation
- Install like any other module
- Use the LazyLoader.config file found in the site root to configure features.
