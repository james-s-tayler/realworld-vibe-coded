using Nuke.Common;

public partial class Build : NukeBuild
{
  public static int Main()
  {
    /*
     * https://github.com/nuke-build/nuke/issues/1088
     * When you try run two nuke builds in parallel, they conflict on the ~/.nuke/temp/build.log
     */

    return Execute<Build>();
  }
}
