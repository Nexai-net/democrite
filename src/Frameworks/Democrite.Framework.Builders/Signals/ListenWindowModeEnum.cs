// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    /// <summary>
    /// Define how internal parameter will use as a fire window
    /// </summary>
    public enum ListenWindowModeEnum
    {
        None,

        /// <summary>
        ///  <br />
        /// Default mode <br />
        ///  <br />
        /// This mode will configured the door to stimulate each time an signal occured <br />
        /// Risk : Signal order could provide invalid fire result. <br />
        ///        Example: A & !B  if A arrive before B it will fire
        /// </summary>
        React,

        /// <summary>
        /// This mode will configured the door to create a timed window base on interval and stimulate only when the window close<br />
        /// This allow all signal time to arrived and provide mode accurate result
        /// </summary>
        Buffered
    }
}
