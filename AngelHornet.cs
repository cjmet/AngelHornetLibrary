﻿using System.Xml.Schema;

namespace AngelHornetLibrary
{
    public static class AngelHornet
    {
        private static string _year { get; set; } = "2024";
        public static string Logo()
        {
            // ASCII Art is (c) All Rights Reserved.
            // Source Code is (c) GPL 3.0
            return """
        ===================================
        @\__    \@@@@@@|\@/|@@@@@@/    __/@
        @@@\__     \@@( + + )@@/     __/@@@
        @@@@@\__     \@) ' (@/     __/@@@@@
        @@@@@@@\______/ @@@ \______/@@@@@@@
        @@@@@@@@@@@@@( +     )@@@@@@@  @@@@
        @@@@@@@@@@@@@@\_/-\_/  @@@@@  @@@@@
        @@@@@@@@@@@@@@@@@@@@@@_______@@@@@@
""" + $"                       (c) AngelHornet {_year}";
        }

        public static string LogoMedium()
        {
            // ASCII Art is (c) All Rights Reserved.
            // Source Code is (c) GPL 3.0
            return """
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@    @@@@@@@@@@@@@*  @@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@# @%     /       %& @@@@@@@@@@@@@@@@@@@@@@@
@#@@@@@@@@@@@@@@@@@@@@@#             #     @@@@@@@@@@@@@@@@@@@@@@ 
@//   @@@@@@@@@@@@@@@@@    .    ./         (@@@@@@@@@@@@@@@@@  . .
@@@  %@   (@@@@@@@@@@@#    @@@ /     &@,,   /@@@@@@@@@@@(   @@  @@
@@@@@& ,,@@@*   (@@@@@/     .              (@@@@@@(.  /@@@&  &@@@@
@@@@      @@@@@@@@   @@@  /            #  &@@@,  @@@@@@@@      @@@
@@@@@@&    #@@@@@@@@@  @   (            . @@  @@@@@@@@@#.   %@@@@@
@@@@@@@@      @@@@@@@@@        @@@@@@    (  @@@@@@@@@      @@@@@@@
@@@@@@@@@@&      @@@@@          @@@@*     . ,@@@@@       @@@@@@@@@
@@@@@@@@@@@@@@@@@@@              @@ /        %  @@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@    @@  .             ,  ,     @@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@      @  ,    %    *            @@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@      @@                        @@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@      @# /    /     * *         /@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@&.     @@ .*@ @@@ .        &@    @@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@       @@@&@@@@% .@@@ @@@@    /@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@#     .   #%@@@@##         %@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@         ,          &@@@@@@@@@@@@@@@@@@@@@@
""" + $"                                              (c) AngelHornet {_year}";
        }

        public static string LogoLarge()
        {
            // ASCII Art is (c) All Rights Reserved.
            // Source Code is (c) GPL 3.0
            return """
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ @@@@@@@@@@@@@@@@@@@@@@@@@ @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%   *@@@@@@@@@@@@@@@@@@@@.  @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&  %   /@@@@@@@@@@@@@@   ,  @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@  @%%                 &@%, @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                           @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
@,   .@@@@@@@@@@@@@@@@@@@@@@@@@%  /             *          @@@@@@@@@@@@@@@@@@@@@@@@@@.    
@@(/ % / ,@@@@@@@@@@@@@@(/@@@@@       //.     //  (      / @@@@@@(/(@@@@@@@@@@@@@/   (( .@
@@@@*  @@@,   #@@@@@@@@@@@@@@@@      &@@, .       @@@#     .@@@@@@@@@@@@@@@@# , .@@@ , @@@
@@@@@@@   @@@@@     *@@@@@@@@@@                  .         /@@@@@@@@@@#     @@@@@   /@@@@@
@@@@@   .(#   @@@@@@@  / . &@@@@  / %/ %       .          (@@&&.@/ .  &@@@@@@%  /..   &@@@
@@@@@@   @(((@@@@@@@@@@@@@/  &@@@.,*   (                 @@@@@   &@@@@@@@@@@@@@((@@  @@@@@
@@@@@@@@@.     #@@@@@@@@@@@@@  %@    /        * *      . @@@  .@@@@@@@@@@@@##     @@@@@@@@
@@@@@@@@@@@@@@@@ . @@@@@@@@@@@/    %      .@@@@@@@           @@@@@@@@@@@   @@@&&@@@@@@@@@@
@@@@@@@@@@@@   &&@@@@@@@@@@@@@@            @@@@@@@          @@@@@@@@@@@@@@&&*  @@@@@@@@@@@
@@@@@@@@@@@@@@@.    ,   *@@@@@             *@@@@@            @@@@@%   .     &@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@* *   .             @@@          .   #   @@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@&&@@@@&                  #              #@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@    #.%@@     /         *                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@        @@       ( .  ,   (  *             @@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@        @@.    *  * (      (   *           &@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@*       *@@                  /               @@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@    #   .@@      @@@@@@@   %           *     @@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@   ,    ,@@@ *  @ @@@@@ ,            @@     @@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@(    (   #@@@@  @@@@@#   @@@@@  /@@@@      @@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@.          &@@.&%( @@@@#/%&%@@@&        @@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@*      /   ,                       (@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#   .(        ,              #@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@(/              //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
""" + $"                                                                      (c) AngelHornet {_year}";
        }

    }
}
