# HR OrgChart Integration


## Entity


PostInfoView


Backend relation:


id

fkParentId



## Adapter


postTreeAdapter


Mapping:


getId()

=> item.id



getParentId()

=> item.fkParentId



## Update Flow



User Drag

    |

validateMove()

    |

moveNode()

    |

Generate UpdatePostCommand

    |

batchUpdatePosts()
