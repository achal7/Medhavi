namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open Expecto
open FsCheck
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Tests.TestCommon

module PropertyTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Property-Based Invariants Tests" [

            testProperty "Property: Sized quantity must always be non-negative" (fun (reqVal: decimal) (lotVal: decimal) ->
                // Guard: focus on valid positive input values
                let reqVal = abs reqVal
                let lotVal = abs lotVal + 0.1m
                
                let req = Quantity.clampToZero reqVal
                let lotSize = Quantity.clampToZero lotVal
                
                let sized = LotSizing.fixedLot lotSize req
                test <@ Quantity.value sized >= 0m @>
            )

            testProperty "Property: FixedLot sizing must yield multiples of lot size" (fun (reqVal: decimal) (lotVal: decimal) ->
                // Guard: positive requirements and non-zero lot size
                let reqVal = abs reqVal
                let lotVal = abs lotVal
                
                if reqVal > 0m && lotVal > 0m then
                    let req = Quantity.clampToZero reqVal
                    let lotSize = Quantity.clampToZero lotVal
                    
                    let sized = LotSizing.fixedLot lotSize req
                    let sizedVal = Quantity.value sized
                    let lotValRef = Quantity.value lotSize
                    
                    test <@ sizedVal % lotValRef = 0m @>
                    test <@ sizedVal >= reqVal @>
            )

            testProperty "Property: Minimum Lot constraint must bound the sized quantity" (fun (reqVal: decimal) (minVal: decimal) ->
                let reqVal = abs reqVal
                let minVal = abs minVal
                
                if reqVal > 0m && minVal > 0m then
                    let req = Quantity.clampToZero reqVal
                    let minQty = Quantity.clampToZero minVal
                    
                    let sized = LotSizing.minimumLot minQty req
                    let sizedVal = Quantity.value sized
                    
                    test <@ sizedVal >= minVal @>
                    test <@ sizedVal >= reqVal @>
            )

            testProperty "Property: Net available balance formula must be consistent" (fun (oh: decimal) (ib: decimal) (res: decimal) (ss: decimal) ->
                let oh = abs oh |> Quantity.clampToZero
                let ib = abs ib |> Quantity.clampToZero
                let res = abs res |> Quantity.clampToZero
                let ss = abs ss |> Quantity.clampToZero
                
                let netAvail = Netting.calculateNetAvailable oh ib res ss
                let netAvailVal = Quantity.value netAvail
                let expected = max 0m ((Quantity.value oh + Quantity.value ib) - (Quantity.value res + Quantity.value ss))
                
                test <@ netAvailVal = expected @>
            )
        ]
