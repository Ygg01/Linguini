﻿namespace Linguini.Parser
{
    public enum ErrorType
    {
        ExpectedToken,
        ExpectedCharRange,
        ExpectedMessageField,
        MissingValue,
        UnbalancedClosingBrace,
        TermAttributeAsPlaceable
    }
}