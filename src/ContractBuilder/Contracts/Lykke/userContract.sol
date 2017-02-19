pragma solidity ^0.4.1;

contract ChronobankAssetProxy {
    function transfer(address _to, uint _value) returns(bool);
}

contract UserContract {
    address _owner;
    address _chronobankAssetProxy;

    modifier onlyowner { if (msg.sender == _owner) _; }

    function UserContract(address chronobankAssetProxy) {
        _owner = msg.sender;
        _chronobankAssetProxy = chronobankAssetProxy;
    }

    function() payable {
        throw;
    }

    function changeChronobankAssetProxy(address assetProxy) onlyowner {
        _chronobankAssetProxy = assetProxy;
    }

    function transferMoney(address recepient, uint value) onlyowner {
        var bank = ChronobankAssetProxy(_chronobankAssetProxy);
        if (!bank.transfer(recepient, value))
            throw;
    }
}