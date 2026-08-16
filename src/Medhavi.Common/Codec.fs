namespace Medhavi.Common

/// CodecError represents failure in encoding or decoding.
type CodecError =
    | EncodeError of string
    | DecodeError of string

/// A Codec is a governed pair of transformations between a semantic value
/// and an external representation.
///
/// Laws:
///   decode (encode x) = Ok x
///   encode (decode y) = Ok y, for all valid representations y
type Codec<'a> =
    { Encode: 'a -> Result<string, CodecError>
      Decode: string -> Result<'a, CodecError> }

module Codec =

    let encode (codec: Codec<'a>) (value: 'a) : Result<string, CodecError> = codec.Encode value

    let decode (codec: Codec<'a>) (representation: string) : Result<'a, CodecError> = codec.Decode representation

    let roundTripLaw (codec: Codec<'a>) (value: 'a) = codec.Encode value |> Result.bind codec.Decode |> (=)(Ok value)
