function DeepCopy(t)
    local newtables = {}
    return Copy(t, newtables)
end

function Copy(t, newtables)
    local newt = {}
    for k, v in pairs(t) do
        if type(v) == 'table' and newtables[v] == nil then
            local tmp = Copy(v)
            newt[k] = tmp
            newtables[tmp] = 1
        else
            newt[k] = v
        end
    end
    return newt
end